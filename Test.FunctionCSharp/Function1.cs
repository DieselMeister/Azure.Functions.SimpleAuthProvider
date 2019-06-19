using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Test.FunctionCSharp
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/auth/token")] HttpRequest req,
            ILogger log)
        {

	        return await (SimpleFunctionsAuthProvider.Functions.authenticate	(req, log));

        }

        [FunctionName("Function2")]
        public static async Task<IActionResult> Run2(
	        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/auth/register")] HttpRequest req,
	        ILogger log)
        {

	        return await (SimpleFunctionsAuthProvider.Functions.createUser(req, log));

        }
	}
}
