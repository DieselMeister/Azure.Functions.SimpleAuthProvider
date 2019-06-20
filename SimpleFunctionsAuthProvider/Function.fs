namespace Azure.Functions.SimpleAuth


module Functions =
    
    open Domain
    open System
    open System.IO
    open Microsoft.Extensions.Primitives
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.Logging
    open System.Web.Http
    open Microsoft.AspNetCore.Mvc
    


    type AuthRequest = {
        App:string option
        UserName:string option
        Password:string option
    }

    type CreateUserRequest = {
        App:string option
        UserName:string option
        Password:string option
        Groups:string option
    }

    type DeleteUserRequest = {
        App:string option
        UserName:string option
    }

    type ChangePasswordRequest = {
        App:string option
        UserName:string option
        OldPassword:string option
        NewPassword:string option
    }

    type ChangeGroupRequest = {
        App:string option
        UserName:string option
        Group:string option
    }

    type TokenResponse = {
        Token:string
        Expires:int
    }

    
    let getConnectionString () =
        System.Environment.GetEnvironmentVariable("AuthStorage")

    let createTableClient () =
        async {
            let connectionStr = getConnectionString ()
            let tableClient =Services.getTableClient connectionStr
            do! DataAccess.createTables(tableClient)
            return tableClient
        }
        

    let toTokenResponse (token:Domain.Token) =
        let totalSecToExpire = (token.ExpiresOn - System.DateTime.UtcNow).TotalSeconds
        {
            Token = token.Token
            Expires =  totalSecToExpire |> int
        }

    let singleStringVal (strVal:StringValues) =
        strVal |> Seq.tryHead


    let getSingleHeaderValue key (headers:IHeaderDictionary) =
        let (hasVal,value) = 
            headers.TryGetValue(key)
        if hasVal then
            value |> singleStringVal
        else
            None
    
    let getCurrentUser (req:HttpRequest) tableClient =
        async {
            let authValue = 
                req.Headers 
                |> getSingleHeaderValue "Authorization"

            match authValue with
            | None ->
                return None
            | Some token ->                
                let! userInfo = tableClient |> Services.getUserInfo token
                return userInfo
        }

    let isUserAdmin (userInfo:UserInfo) =
        userInfo.Groups                         
        |> List.exists (fun i -> 
            String.Equals(i,"administrator",StringComparison.InvariantCultureIgnoreCase)
        )


    let currentUserIsAdmin (req:HttpRequest) tableClient =
        async {
            let! userInfo = getCurrentUser req tableClient
            match userInfo with
            | None -> return false
            | Some userInfo ->
                return userInfo |> isUserAdmin
        }
        


    let createUser (req:HttpRequest) (log:ILogger) =
        async {
            use sr = new StreamReader(req.Body)
            let! bodyContent = sr.ReadToEndAsync() |> Async.AwaitTask
            let request = FSharp.Json.Json.deserialize<CreateUserRequest> bodyContent
            // check if current User is administrator
            let! tableClient = createTableClient ()
            let! isCurrentUserAdmin = tableClient |> currentUserIsAdmin req 
            if not isCurrentUserAdmin then
                log.LogError("only admin is authorized to add a new user!")
                return UnauthorizedResult() :> IActionResult
            else    
                match request.App,request.UserName,request.Password with
                | Some app, Some username, Some password ->
                    let groups = request.Groups |> Option.defaultValue ""
                    let! newUser = 
                        tableClient
                        |> Services.createUser app username password groups
                    match newUser with
                    | None ->
                        return InternalServerErrorResult() :> IActionResult
                    | Some _ ->
                        return OkResult() :> IActionResult
                | _ ->
                    return BadRequestErrorMessageResult("app, username and password needed") :> IActionResult
        } |> Async.StartAsTask


    let deleteUser (req:HttpRequest) (log:ILogger) =
        async {
            use sr = new StreamReader(req.Body)
            let! bodyContent = sr.ReadToEndAsync() |> Async.AwaitTask
            let request = FSharp.Json.Json.deserialize<DeleteUserRequest> bodyContent

            // check if current User is administrator
            let! tableClient = createTableClient ()
            let! isCurrentUserAdmin = tableClient |> currentUserIsAdmin req 
            if not isCurrentUserAdmin then
                log.LogError("only admin is authorized to delete a user!")
                return UnauthorizedResult() :> IActionResult
            else    
                match request.App,request.UserName with
                | Some app, Some username ->                    
                    let! isDeleted = 
                        tableClient
                        |> Services.deleteUser app username
                    match isDeleted with
                    | false ->
                        log.LogError("failed to delete user!")
                        return InternalServerErrorResult() :> IActionResult
                    | true ->
                        return OkResult() :> IActionResult
                | _ ->
                    return BadRequestErrorMessageResult("app, username and old and new password needed") :> IActionResult
        } |> Async.StartAsTask


    let changePassword (req:HttpRequest) (log:ILogger) =
        async {
            use sr = new StreamReader(req.Body)
            let! bodyContent = sr.ReadToEndAsync() |> Async.AwaitTask
            let request = FSharp.Json.Json.deserialize<ChangePasswordRequest> bodyContent
            match request.App,request.UserName,request.OldPassword,request.NewPassword with
            | Some app, Some username, Some oldPassword, Some newPassword ->
                let! tableClient = createTableClient ()
                let! currentUser = getCurrentUser req tableClient
                let! toChangeUser = 
                    DataAccess.getUserFromUsername tableClient app username
                match currentUser, toChangeUser with
                | Some currentUser, Some toChangeUser ->
                    // only admin or user himself can change password
                    if (isUserAdmin currentUser) || 
                        (toChangeUser.UserName = currentUser.UserName) then
                        let! hasChanged = Services.changePassword app username oldPassword newPassword tableClient
                        if hasChanged then
                            return OkResult() :> IActionResult
                        else
                            log.LogError("error changing password")
                            return InternalServerErrorResult() :> IActionResult
                    else
                        log.LogDebug("user not authenticated to change password")
                        return UnauthorizedResult() :> IActionResult
                | _ ->
                    log.LogDebug("can not determinate user")
                    return InternalServerErrorResult() :> IActionResult
            | _ ->
                return BadRequestErrorMessageResult("app, username and password needed") :> IActionResult
        } |> Async.StartAsTask


    let addGroupToUser (req:HttpRequest) (log:ILogger) =
        async {
            use sr = new StreamReader(req.Body)
            let! bodyContent = sr.ReadToEndAsync() |> Async.AwaitTask
            let request = FSharp.Json.Json.deserialize<ChangeGroupRequest> bodyContent

            // check if current User is administrator
            let! tableClient = createTableClient ()
            let! isCurrentUserAdmin = tableClient |> currentUserIsAdmin req 
            if not isCurrentUserAdmin then
                log.LogError("only admin is authorized to add a group to a user!")
                return UnauthorizedResult() :> IActionResult
            else    
                match request.App,request.UserName,request.Group with
                | Some app, Some username,Some group ->                    
                    let! hasChanged = 
                        tableClient
                        |> Services.addGroupToUser app username group
                    match hasChanged with
                    | false ->
                        log.LogError("failed to add a group to the user!")
                        return InternalServerErrorResult() :> IActionResult
                    | true ->
                        return OkResult() :> IActionResult
                | _ ->
                    return BadRequestErrorMessageResult("app, username and group needed") :> IActionResult
        } |> Async.StartAsTask


    let removeGroupFromUser (req:HttpRequest) (log:ILogger) =
        async {
            use sr = new StreamReader(req.Body)
            let! bodyContent = sr.ReadToEndAsync() |> Async.AwaitTask
            let request = FSharp.Json.Json.deserialize<ChangeGroupRequest> bodyContent

            // check if current User is administrator
            let! tableClient = createTableClient ()
            let! isCurrentUserAdmin = tableClient |> currentUserIsAdmin req 
            if not isCurrentUserAdmin then
                log.LogError("only admin is authorized to remove a group from a user!")
                return UnauthorizedResult() :> IActionResult
            else    
                match request.App,request.UserName,request.Group with
                | Some app, Some username,Some group ->                    
                    let! hasChanged = 
                        tableClient
                        |> Services.removeGroupFromUser app username group
                    match hasChanged with
                    | false ->
                        log.LogError("failed to remove a group from the user!")
                        return InternalServerErrorResult() :> IActionResult
                    | true ->
                        return OkResult() :> IActionResult
                | _ ->
                    return BadRequestErrorMessageResult("app, username and group needed") :> IActionResult
        } |> Async.StartAsTask


    
    let authenticate (req:HttpRequest) (log:ILogger) =
        async {
            use sr = new StreamReader(req.Body)
            let! bodyContent = sr.ReadToEndAsync() |> Async.AwaitTask
            let request = FSharp.Json.Json.deserialize<AuthRequest> bodyContent
            //let request = JsonConvert.DeserializeObject<AuthRequest>(bodyContent,serializerSettings)
            match request.App,request.UserName,request.Password with
            | Some app, Some username, Some password ->
                let! tableClient = createTableClient ()
                let! token = 
                    tableClient
                    |> Services.authenticate app username password
                match token with
                | None ->
                    return UnauthorizedResult() :> IActionResult
                | Some token ->
                    let resp = token |> toTokenResponse
                    return OkObjectResult(resp) :> IActionResult
            | _ ->
                return BadRequestErrorMessageResult("app, username and password needed") :> IActionResult


        } |> Async.StartAsTask
            
       

    // check if session token is valid "api/auth/token/validate" payload app token
    let validate (req:HttpRequest) (log:ILogger) =
        async {
            let authValue = 
                req.Headers 
                |> getSingleHeaderValue "Authorization"

            match authValue with
            | None ->
                return UnauthorizedResult() :> IActionResult
            | Some token ->
                let! tableClient = createTableClient ()
                let! isValid = tableClient |> Services.validate token
                return
                    if isValid then
                        OkResult() :> IActionResult
                    else
                        UnauthorizedResult() :> IActionResult
                    
        } |> Async.StartAsTask
        
        

    // invalidate token "api/auth/token/invalidate" payload token and app

    let invalidate (req:HttpRequest) (log:ILogger) =
        async {
            let authValue = 
                req.Headers 
                |> getSingleHeaderValue "Authorization"

            match authValue with
            | None ->
                return UnauthorizedResult() :> IActionResult
            | Some token ->
                let! tableClient = createTableClient ()
                do! tableClient |> Services.invalidate token
                return OkResult() :> IActionResult
        } |> Async.StartAsTask
        
    
    // get user info from token "api/auth/userinfo"
    let getUserInfo (req:HttpRequest) (log:ILogger) =
        async {
            let authValue = 
                req.Headers 
                |> getSingleHeaderValue "Authorization"

            match authValue with
            | None ->
                return UnauthorizedResult() :> IActionResult
            | Some token ->
                let! tableClient = createTableClient ()
                let! user = tableClient |> Services.getUserInfo token
                return
                    match user with
                    | None ->
                        UnauthorizedResult() :> IActionResult
                    | Some user->
                        OkObjectResult(user) :> IActionResult
        } |> Async.StartAsTask


    // create an user. Please remove this endpoint!
    let createEmptyUserForInit (req:HttpRequest) (log:ILogger) =
        async {
            use sr = new StreamReader(req.Body)
            let! bodyContent = sr.ReadToEndAsync() |> Async.AwaitTask
            if bodyContent <> "I know what i am doing here!" then
                return UnauthorizedResult() :> IActionResult
            else
                let! tableClient = createTableClient ()

                let! hasUsers = DataAccess.hasUsers tableClient
                if hasUsers then
                    return BadRequestErrorMessageResult("you have already users. Why do you haven't deleted this endpoint?!") :> IActionResult
                else
                    let! alreadyEmptyUser = Services.checkUserName "" "emptyUser" tableClient
                    if alreadyEmptyUser then
                        return BadRequestErrorMessageResult("empty user already exists. Why do you haven't deleted this endpoint?!") :> IActionResult
                    else
                        let! newUser = 
                            tableClient
                            |> Services.createUser "" "emptyUser" "secret" ""
                        match newUser with
                        | None ->
                            return InternalServerErrorResult() :> IActionResult
                        | Some token ->
                            return OkResult() :> IActionResult
        } |> Async.StartAsTask

    
        

    

    


        





