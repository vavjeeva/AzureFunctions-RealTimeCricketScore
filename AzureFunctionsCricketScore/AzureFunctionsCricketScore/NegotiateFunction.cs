
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace AzureFunctionsCricketScore
{
    public static class NegotiateFunction
    {
        [FunctionName("negotiate")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req,
            [SignalRConnectionInfo(HubName = "broadcasthub")]SignalRConnectionInfo info, ILogger log)
        {
            return info != null
                ? (ActionResult)new OkObjectResult(info)
                : new NotFoundObjectResult("Failed to load SignalR Info.");
        }
    }
}
