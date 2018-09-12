using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctionsCricketScore
{
    public static class BroadcastFunction
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("broadcast")]
        public static async void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer,
            [SignalR(HubName = "broadcasthub")]IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //TODO: API key should be stored in Azure Key Vault .
            //For Demo purpose, i hard coded the value here.
            var values = new Dictionary<string, string>
            {
                //Hard coded Cricket Match Unique ID. You can change the Match id based on ongoing matchers
              {"apikey", "_API_KEY_HERE"},{"unique_id", "1119553"}
            };

            using (var response = httpClient.PostAsJsonAsync(new Uri("http://cricapi.com/api/cricketScore/"), values).Result)
            {
                var resultObj = response.Content.ReadAsStringAsync().Result;
                dynamic result = JsonConvert.DeserializeObject(resultObj);


                await signalRMessages.AddAsync(new SignalRMessage()
                {
                    Target = "broadcastData",
                    Arguments = new object[] { result.score }
                });
            }
        }
    }
}
