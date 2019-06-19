module DaFunc 

    open Microsoft.Azure.WebJobs
    open Microsoft.AspNetCore.Http
    open Azure.Functions.SimpleAuth


    [<FunctionName("Authenticate")>]
    let authenticate
        (
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "post", Route="api/auth/token")>] req,
        log
        ) = Functions.authenticate req log


    [<FunctionName("CreateUser")>]
    let createUser
        (
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "post", Route="api/auth/register")>] req,
        log
        ) = Functions.createUser req log

    [<FunctionName("DeleteUser")>]
    let deleteUser
        (
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "post", Route="api/auth/delete")>] req,
        log
        ) = Functions.deleteUser req log

    [<FunctionName("ChangePassword")>]
    let changePassword
        (
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "post", Route="api/auth/changepassword")>] req,
        log
        ) = Functions.changePassword req log

    [<FunctionName("Validate")>]
    let validate
        (
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "get", Route="api/auth/validate")>] req,
        log
        ) = Functions.validate req log


    [<FunctionName("InValidate")>]
    let invalidate
        (
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "delete", Route="api/auth/invalidate")>] req,
        log
        ) = Functions.invalidate req log


    [<FunctionName("UserInfo")>]
    let getUserInfo
        (
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "get", Route="api/auth/userinfo")>] req,
        log
        ) = Functions.getUserInfo req log


    [<FunctionName("EmptyUserInit")>]
    let createEmptyUserForInit
        (
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "post", Route="api/auth/emptyuserinit")>] req,
        log
        ) = Functions.createEmptyUserForInit req log



            

