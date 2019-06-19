namespace Azure.Functions.SimpleAuth

module Services =

    let getTableClient connectionString =
        let account = DataAccess.initStorageAccount connectionString
        DataAccess.initTableClient account


    let checkUserName app username tableClient =
        async {
            let! user = username |> DataAccess.getUserFromUsername tableClient app
            return user <> None
        }
        

    let createUser app username password groups tableClient =
        let getUser = DataAccess.getUserFromUsername tableClient
        let addNewUser = DataAccess.addNewUser tableClient
        Domain.Authentication.createUser getUser addNewUser app username password groups

    let deleteUser app username tableClient =
        let getUser = DataAccess.getUserFromUsername tableClient
        let deleteUser = DataAccess.deleteUser tableClient
        Domain.Authentication.deleteUser getUser deleteUser app username

    let changePassword app username oldPassword newPassword tableClient =
        let getUser = DataAccess.getUserFromUsername tableClient
        let updateUser = DataAccess.updateUser tableClient
        Domain.Authentication.changePassword getUser updateUser app username oldPassword newPassword


    // api functions

    // auth user with name and password (get Session token)   "api/auth/token"
    let authenticate app username password tableClient =
        let getUser = DataAccess.getUserFromUsername tableClient
        let storeToken = DataAccess.storeToken tableClient
        Domain.Authentication.authenticate getUser storeToken app username password
        

    // check if session token is valid "api/auth/token/validate" payload app token
    let validate token tableClient =
        let getToken = DataAccess.getToken tableClient
        Domain.Authentication.validate getToken token
        

    // invalidate token "api/auth/token/invalidate" payload token and app

    let invalidate token tableClient =
        let getToken = DataAccess.getToken tableClient
        let removeToken = DataAccess.removeToken tableClient
        Domain.Authentication.invalidate getToken removeToken token
        
    
    // get user info from token "api/auth/userinfo"
    let getUserInfo token tableClient =
        let getToken = DataAccess.getToken tableClient
        let getUser = DataAccess.getUserFromUsername tableClient
        Domain.Authentication.getUserInfo getToken getUser token

