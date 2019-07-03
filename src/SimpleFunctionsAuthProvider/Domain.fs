namespace Azure.Functions.SimpleAuth

module Domain =
    
    open System
    open System.Security.Cryptography
    open Microsoft.AspNetCore.Cryptography.KeyDerivation
    open FSharp.Azure.Storage.Table    

    let standardExpirationTime = 36000

    type Errors =
        | ValidationError of string
        | UserAlreadExisitsError
        | InfrastructurError


    type User = {
        [<PartitionKey>] AppName: string
        [<RowKey>] UserName:string
        PasswordHash:string
        Salt:string
        Groups:string
    }

    type Token = {
        [<PartitionKey>] UserName: string
        [<RowKey>] Token:string
        App:string
        ExpiresOn:DateTime
    }

    type UserInfo = {
        UserName:string
        Groups: string list
    }

    let private generateRandomBytes length =
        let buffer:byte [] = Array.zeroCreate (length)
        use rng = RandomNumberGenerator.Create()
        rng.GetBytes(buffer)
        buffer

    let private byteToString (bytes: byte []) =
        Convert.ToBase64String(bytes)

    let private stringToBytes (str:string) =
        Convert.FromBase64String(str)

    let private splitGroupString (str:string) =
        str.Split([|" "|],StringSplitOptions.RemoveEmptyEntries)
        |> List.ofArray

    let private buildGroupString (groups:string list) =
        String.Join(" ",groups)

    let private removeInvalidCharacters (str:String) =
        str
        |> Seq.filter (fun c -> c<>'/' && c<> '\\' && c<> '#' && c<> '?')
        |> Seq.filter (fun c -> 
            let cByte = (c |> byte)
            let firstCondition = cByte >= 0x00uy && cByte <= 0x1Fuy
            let secondCondition = cByte >= 0x7Fuy && cByte <= 0x9fuy
            not (firstCondition || secondCondition)
        )
        |> Seq.filter (fun c -> c<>'\r' && c<> '\n' && c<> '\t')
        |> Seq.toArray
        |> String

    let private hasInvalidCharacters (str:String) =
        str <> (str |> removeInvalidCharacters)



    module private User =

        let toUserInfo (user:User) =
            { 
                UserName = user.UserName
                Groups = 
                    user.Groups |> splitGroupString
            }


    module private Password =


        let private createSalt () =
            generateRandomBytes (128/8)

            
        let private hashPassword salt pw =
            KeyDerivation.Pbkdf2(pw,salt,KeyDerivationPrf.HMACSHA512,10000, 256 / 8)
            

        let createHashWithSalt pw =
            let salt = createSalt()
            let hashedPw = pw |> hashPassword salt
            (Convert.ToBase64String(hashedPw),Convert.ToBase64String(salt))
            
        
        let check saltStr hashStr pwStr =
            let salt = Convert.FromBase64String(saltStr)
            let hash = Convert.FromBase64String(hashStr)
            let expectedHash = pwStr |> hashPassword salt
            expectedHash = hash
    


    module private Token =

        let generate (expiresIn:int) app username =
            let expiresOn = DateTime.UtcNow.AddSeconds(expiresIn |> float)
            let token = 
                generateRandomBytes 48 
                |> byteToString
                |> removeInvalidCharacters

            { 
                UserName = username
                Token = token
                App = app
                ExpiresOn = expiresOn 
            }

        let isExpired (token:Token) =
            let currentTime = DateTime.UtcNow
            token.ExpiresOn < currentTime


    module Authentication =

        let createUser getUser addNewUser app username password groups =
            async {
                if app |> hasInvalidCharacters then
                    return ValidationError(@"app name has invalid character. # ? / \ are not allowed!") |> Error
                elif username |> hasInvalidCharacters then
                    return ValidationError(@"username has invalid character. # ? / \ are not allowed!") |> Error
                elif username = "" then
                    return ValidationError("username is empty") |> Error
                else
                    let! user = getUser app username
                    match user with
                    | Some _ -> return Error UserAlreadExisitsError
                    | None -> 
                        let (pwHash,salt) = Password.createHashWithSalt password
                        let newUser = {
                            AppName = app
                            UserName = username
                            PasswordHash = pwHash
                            Salt = salt
                            Groups = groups
                        }
                        let! addResult = newUser |> addNewUser
                        match addResult with
                        | None -> return Error InfrastructurError
                        | Some _ ->
                            return Ok newUser
            }


        let deleteUser getUser deleteUser app username =
            async {
                let! user = getUser app username
                match user with
                | None -> return false
                | Some user ->
                    do! user |> deleteUser
                    return true
            }


        let changePassword getUser updateUser app username oldPassword newPassword =
            async {
                let! user = getUser app username
                match user with
                | None -> return false
                | Some user ->
                    // check Old Password
                    let isOldOk = 
                        Password.check user.Salt user.PasswordHash oldPassword
                    if  not isOldOk then
                        return false
                    else
                        let (pwHash,salt) = 
                            Password.createHashWithSalt newPassword
                        let newUser = {
                            user with
                                PasswordHash = pwHash
                                Salt = salt
                        }
                        do! newUser |> updateUser
                        return true
            }


        let addGroupToUser getUser updateUser app username group =
            async {
                let! (user:User option) = getUser app username
                match user with
                | None -> return false
                | Some user ->
                    
                    let newGroups = 
                        (user.Groups |> splitGroupString)
                        @ [ group ]
                        |> List.distinct // remove duplicate

                    let newUser =
                        { user with Groups = newGroups |> buildGroupString }
                    do! newUser |> updateUser
                    return true
            }



        let removeGroupFromUser getUser updateUser app username group =
            async {
                let! (user:User option) = getUser app username
                match user with
                | None -> return false
                | Some user ->
                    let newGroups = 
                        user.Groups 
                        |> splitGroupString
                        |> List.filter (fun i -> i<>group)
                    let newUser =
                        { user with Groups = newGroups |> buildGroupString }
                    do! newUser |> updateUser
                    return true
            }


        let authenticate getUser storeToken app username password  =
            async {
                let! user = 
                    username 
                    |> getUser app
                match user with
                | None -> return None
                | Some user ->
                    if Password.check user.Salt user.PasswordHash password then
                        let newToken = Token.generate standardExpirationTime app username
                        let! dbResult = newToken |> storeToken 
                        return dbResult |> Option.map (fun _ -> newToken)
                    else
                        return None
            }


        let validate getToken token =
            async {
                let! token = getToken token
                match token with
                | None -> return false
                | Some token ->
                    return token |> Token.isExpired |> not
            }


        let invalidate getToken removeToken token =
            async {
                let! token = getToken token
                match token with
                | None -> return ()
                | Some token ->
                    let! dbResult = token |> removeToken
                    match dbResult with
                    | None -> failwith "error removing token"
                    | Some _ -> return ()
            }

        
        let getUserInfo getToken getUser token =
            async {
                let! token = token |> getToken
                match token with
                | None -> return None
                | Some token ->
                    if token |> Token.isExpired then                        
                        return None
                    else
                        let! user = token.UserName |> getUser token.App
                        return user |> Option.map (fun u -> u |> User.toUserInfo)
            }



    
    


    


    
        

