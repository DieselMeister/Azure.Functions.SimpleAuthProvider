namespace Azure.Functions.SimpleAuth

module DataAccess =
    
    open Domain
    open Microsoft.WindowsAzure.Storage
    open Microsoft.WindowsAzure.Storage.Table
    open FSharp.Azure.Storage.Table

    let userTableName = "Users"
    let tokenTableName = "Token"


    let initStorageAccount connectionString =
        CloudStorageAccount.Parse connectionString

    let initTableClient (account:CloudStorageAccount) = 
        account.CreateCloudTableClient()


    let private inUserTable tableClient user = inTableAsync tableClient userTableName user


    let private inTokenTable tableClient token = inTableAsync tableClient tokenTableName token


    let private fromUserTable tableClient q = fromTableAsync tableClient userTableName q


    let private fromTokenTable tableClient q = fromTableAsync tableClient tokenTableName q


    let createTables (tableClient:CloudTableClient) =
        async {
            let tableRef = tableClient.GetTableReference("Users")
            let! isCreated = tableRef.CreateIfNotExistsAsync() |> Async.AwaitTask
            let tableRef2 = tableClient.GetTableReference("Token")
            let! isCreated2 = tableRef2.CreateIfNotExistsAsync() |> Async.AwaitTask
            return ()
        }


    let private getUserFromUsernameWithMeta tableClient app username =
        async {
            let! result =
                Query.all<User>
                |> Query.where <@ fun _ s -> s.PartitionKey = app && s.RowKey = username @>
                |> fromUserTable tableClient

            return 
                result                 
                |> Seq.tryHead
        }

    let getUserFromUsername tableClient app username =
        async {
            let! result =
                getUserFromUsernameWithMeta tableClient app username

            let user = 
                result                 
                |> Option.map (fun (u,_) -> u)

            return user
        }


    let addNewUser tableClient user =
        async {
            let! result = user |> Insert |> inUserTable tableClient
            
            if result.HttpStatusCode = 204 && result.Etag <> "" then
                return Some ()
            else
                return None

        }

    let deleteUser tableClient user =
        async {
            let! dbUser = getUserFromUsernameWithMeta tableClient user.AppName user.UserName
            match dbUser with
            | None -> return ()
            | Some (user,meta) ->
                let! result = (user,meta.Etag) |> Delete |> inUserTable tableClient
                if result.HttpStatusCode = 204 then
                    return ()
                else
                    failwith "error deleting user!"

        }

    let updateUser tableClient user =
        async {
            let! dbUser = getUserFromUsernameWithMeta tableClient user.AppName user.UserName
            match dbUser with
            | None -> return ()
            | Some (_,meta) ->
                let! result = (user,meta.Etag) |> Replace |> inUserTable tableClient
                if result.HttpStatusCode = 204 then
                    return ()
                else
                    failwith "error updating user!"

        }

    let getAllExpiredToken tableClient time =
        async {
            let! expiredToken =
                Query.all<Token>
                |> Query.where <@ fun t s -> t.ExpiresOn < time @>
                |> fromTokenTable tableClient

            return expiredToken
        }


    let private getTokenWithMeta tableClient token =
        async {
            let! result =
                Query.all<Token>
                |> Query.where <@ fun t s -> t.Token = token @>
                |> fromTokenTable tableClient

            return 
                result                 
                |> Seq.tryHead
        }

    let getToken tableClient token =
        async {
            let! result =
                getTokenWithMeta tableClient token

            return 
                result 
                |> Option.map (fun (t,_) -> t)
                
        }
        
    let storeToken tableClient (token:Token) =
        async {
            
            let! result = token |> Insert |> inTokenTable tableClient
            if result.HttpStatusCode = 204 && result.Etag <> "" then
                return Some token
            else
                return None
        }

    let removeToken tableClient (token:Token) =
        async {
            let! dbToken = token.Token |> getTokenWithMeta tableClient
            match dbToken with
            | None -> return None
            | Some (t,meta) ->
                let! result = (t,meta.Etag) |> Delete |> inTokenTable tableClient
                if result.HttpStatusCode = 204 then
                    return Some token
                else
                    return None
        }

    let hasUsers tableClient =
        async {
            let! result =
                Query.all<User>                
                |> fromTokenTable tableClient

            return result |> Seq.isEmpty |> not
        }
