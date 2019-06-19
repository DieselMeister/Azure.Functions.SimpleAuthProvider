# A very simple Authentication Provider for deploying on Azure Functions

Motivation: Sometimes I need for a very simple web application, which is basically made up with only an Azure Storage Static Page and an Azure Function as Backend

So a full blown Identity Server with OAuth is sometime to heavy for the customer and not needed, because the app which are "secured" doesn't contain any personal info. 
Mostly they want to "hide" these small apps from other companies.

To host these simple web apps on azure storage as static site is cheap. But with Fable and Elmish you can build greate UI's.
Also the Azure Function are cheap to host and mostly enough for that kind of applications.

So I build that "Provider", "Wrapper" or something, where you can add some basic authentication functionality to you azure functions app.

* GetToken
* Validate Token
* Invalidate Token
* Get UserInfo
* Create User
* Delete User
* Change Password

You have to write the endpoint for yourself, so you can choose, what you need.

*Please Note: Even if I uses the same hash and salt generation for the password, I did build a full blown bullet prove OAuth. 
If you have personal information from yout customers, you want to secure, please considerto use an proper OAuth Provider.
Do not be cheap on security.*

Nevertheless, this simpel provider does his work.

## What you need.

1. Please fire up an azure function Project. (F# or C# or VB.Net, watever you want.)

2. Implement following Functions:
* you do not need all of them. Choose the one, you need.
* you can change the endpoints as well

(a sample App is in the repo !)

### F#

```fs
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
```

### C#

```cs

    public static class AuthenticationStuff
    {

	    [FunctionName("Authenticate")]
	    public static Task<IActionResult> Authenticate(
		    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/auth/token")]
		    HttpRequest req,
		    ILogger log)
		    => Functions.authenticate(req, log);

		[FunctionName("CreateUser")]
		public static Task<IActionResult> CreateUser(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/auth/register")]
			HttpRequest req,
			ILogger log)
			=> Functions.createUser(req, log);

		[FunctionName("DeleteUser")]
		public static Task<IActionResult> DeleteUser(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route="api/auth/delete")]
			HttpRequest req,
			ILogger log)
			=> Functions.deleteUser(req, log);

		[FunctionName("ChangePassword")]
		public static Task<IActionResult> ChangePassword(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/auth/changepassword")]
			HttpRequest req,
			ILogger log)
			=> Functions.changePassword(req, log);

		[FunctionName("Validate")]
		public static Task<IActionResult> Validate(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="api/auth/validate")]
			HttpRequest req,
			ILogger log)
			=> Functions.validate(req, log);

		[FunctionName("InValidate")]
		public static Task<IActionResult> InValidate(
			[HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route="api/auth/invalidate")]
			HttpRequest req,
			ILogger log)
			=> Functions.invalidate(req, log);

		[FunctionName("UserInfo")]
		public static Task<IActionResult> UserInfo(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="api/auth/userinfo")]
			HttpRequest req,
			ILogger log)
			=> Functions.getUserInfo(req, log);

		[FunctionName("EmptyUserInit")]
		public static Task<IActionResult> EmptyUserInit(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route="api/auth/emptyuserinit")]
			HttpRequest req,
			ILogger log)
			=> Functions.createEmptyUserForInit(req, log);
	}

```
