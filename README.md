# A very simple Authentication Provider for deploying on Azure Functions

**Attention: The implementation maybe has some flaws in compaison with for example a full blown OAUTH implementation, which I address. It is a pre alpha version ,if you will. I am happy, if you have some "Issues", Ideas or critics or some nice words.**

Motivation: Sometimes I need for a very simple web application, which is basically made up with only an Azure Storage Static Page and an Azure Function as Backend, a simple authentication mechanism. Backic Auth on the azure storage static website is currently not possible.

So a full blown Identity Server with OAuth is sometime to heavy for the customer and not needed, because the app which are "secured" doesn't contain any personal or secret informations. 
Mostly they want to "hide" these small apps from other companies.

To host these simple web apps on azure storage as static site is cheap. But with Fable and Elmish you can build greate UI's.
Also the Azure Function are cheap to host and mostly enough for that kind of applications.

So I build that "Provider", "Wrapper" or name it for yourself (Service?), where you can add some basic authentication functionality to you azure functions app.

* GetToken
* Validate Token
* Invalidate Token
* Get UserInfo
* Create User
* Delete User
* Change Password
* Add Group To User
* Remove Group From User

You have to write the endpoint functions for yourself, so you can choose, what you need.

**Please Note: Even if I uses the same hash and salt generation for the password as in asp.net core, I didn't build a full blown bullet prove OAuth Provider. 
If you have personal information from your customers, you want to secure, please consider to use an proper OAuth Provider.
Do not be cheap on security.**

Nevertheless, this simpel provider does his work. And sometimes it is enough to "secure" this little web app, you have written for you electician, builder, hair stylist or nail studio next to you :)

## Nuget

You can find the Package on Nuget.

```
dotnet add package SimpleAuthProvider.Azure.Functions
```

## What you need.

1. Please fire up an azure function Project. (F# or C# or VB.Net, whatever you want.)

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
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "post", Route="api/auth/user/delete")>] req,
        log
        ) = Functions.deleteUser req log

    [<FunctionName("ChangePassword")>]
    let changePassword
        (
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "post", Route="api/auth/user/changepassword")>] req,
        log
        ) = Functions.changePassword req log

    [<FunctionName("AddGroupToUser")>]
    let addGroupToUser
        (
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "post", Route="api/auth/user/addgroup")>] req,
        log
        ) = Functions.addGroupToUser req log

    [<FunctionName("RemoveGroupFromUser")>]
    let removeGroupFromUser
        (
        [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, "post", Route="api/auth/user/removegroup")>] req,
        log
        ) = Functions.removeGroupFromUser req log

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
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route="api/auth/user/delete")]
			HttpRequest req,
			ILogger log)
			=> Functions.deleteUser(req, log);

		[FunctionName("ChangePassword")]
		public static Task<IActionResult> ChangePassword(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/auth/user/changepassword")]
			HttpRequest req,
			ILogger log)
			=> Functions.changePassword(req, log);


		[FunctionName("AddGroupToUser")]
		public static Task<IActionResult> AddGroupToUser(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/auth/user/addgroup")]
			HttpRequest req,
			ILogger log)
			=> Functions.addGroupToUser(req, log);


		[FunctionName("RemoveGroupFromUser")]
		public static Task<IActionResult> RemoveGroupFromUser(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/auth/user/removegroup")]
			HttpRequest req,
			ILogger log)
			=> Functions.removeGroupFromUser(req, log);



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


## Settings

You have to set the "AuthStorage" to point to the Azure Table Storage of you choice. Mostly it's the same you use for your other functions.

```json
{
    "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "AuthStorage": "UseDevelopmentStorage=true"
  }
}
```

## The Interface/Endpoints

I communicate via JSON with the endpoints. So The Data like "Passwords" and "Usernames" etc. pp will mostly posted as JSON.
The Authorization Token is send by set the HTTP Header: "Authorization (here your token!)".

### CreateUser Endpoint (POST)

Users can only be created if you have a token from a user which is in the group admin.
*For the first time, you need to add one youer, than you can change add the group "administrator" (split by spaces) with the azure storage explorer on this "first" user. And than you can build up you stuff. For that there is a special enpoint to add the first user!*

Back to Create User:

Header:
```Authorization (here you token!)```

Body:
```json
{
    "App": "MyApp",
    "UserName": "theUser",
    "Password": "secret",
    "Groups":"group1 group2"
}
```

### DeleteUser Endpoint (POST)

Same as Create, you can only delete a user as an administrator.

Header:
Authorization (here you token!)

Body:
```json
{
    "App": "MyApp",
    "UserName": "theUser"
}
```

### ChangePassword Endpoint (POST)

You can only change a password as administrator or as yourself

Header:
```Authorization (here you token!)```

Body:
```json
{
    "App": "MyApp",
    "UserName": "theUser",
    "OldPassword": "secret",
    "NewPassword":"new secret"
}
```


### Add Group To User Endpoint (POST)

Adds a group to a user. Only as an administrator you can do that.

Header:
Authorization (here you token!)

Body:
```json
{
    "App": "MyApp",
    "UserName": "theUser",
    "Group":"newGroup"
}
```


### Remove Group From User Endpoint (POST)

Removes a group from a user. Only as an administrator you can do that.

Header:
Authorization (here you token!)

Body:
```json
{
    "App": "MyApp",
    "UserName": "theUser",
    "Group":"notNeedGroup"
}
```


### Authenticate (getToken) Endpoint (POST)

Here you authenticate the user with a password and you get a session token.

Body:
```json
{
    "App": "MyApp",
    "UserName": "theUser",
    "Password": "secret"
}
```

Response:
```json
{
    "token": "1Tu7PhNyzlBkyK+7k2U3QxDPm1Rg0UkwGegEiVpfxlFYOSHXAGkUn4dn2rzEa+Q0t+wM02d6pK8IMXBStATCLQ==",
    "expires": 35999
}
```

### Validate Token Endpoint (GET)

Here you can check, if a token is valid.


Header:
```Authorization (here you token!)```

You get 200 or 401


### Invalidate Token Endpoint (DELETE)

Here you can invalidate a Token. After that the Token is no longer available.

Header:
```Authorization (here you token!)```

You get always a 200.


### UserInfo Endpoint (GET)

Here you can get the Userinfo. The Name and The Group in which he/she is in.

Header:
```Authorization (here you token!)```

Response:
```json
{
    "userName": "emptyUser",
    "groups": [
    	"group 1",
	"group 2"
    ]
}
```

### The Create Your First User Endpoint (POST)
It create an user without an app name and without any group.
The name is "emptyUser" and password is "secret".
You can "extend" the user with Azure Storage Explorer and make it a base for further setup.

In the Body you have to type (with out the "):

"I know what i am doing here!"

And if you do not have any user, the user will be created. Otherwise you get a proper message.

Please remove the endpoint, if you do not need it any more!

Body:
```
I know what i am doing here!
```
