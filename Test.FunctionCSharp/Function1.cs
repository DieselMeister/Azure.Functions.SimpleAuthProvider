using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Functions.SimpleAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Test.FunctionCSharp
{
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
}



