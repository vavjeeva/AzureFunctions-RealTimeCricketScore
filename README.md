Real-time cricket score notifications from a Chrome extension using serverless Azure Functions and Azure SignalR. 
I have used cricapi.com free API service to get the live cricket score updates. 

# Register Cricket Services API

As a first step, to consume the API Service from cricapi.com, register the account with the details to get the API Key. They allow 100 free hits per day for the testing purposes. 
 
# Creating Azure SignalR Service

Log into your Azure Portal (https://portal.azure.com/) and create a new resource of type SignalR Service. After the service is created, copy the connection string from the Keys section. 

# Creating Azure Function App

## NegotiateFuntion.cs 
```csharp
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
```
## BroadCastFunction.cs 
```csharp
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
```

# Creating Chrome Extension

## SignalRClient.js

```javascript
const apiBaseUrl = 'https://azurefunctionscricketscore20180911095957.azurewebsites.net';  
const hubName = 'broadcasthub';  
  
getConnectionInfo().then(info => {  
  const options = {  
    accessTokenFactory: () => info.accessKey  
    };  
    
  const connection = new signalR.HubConnectionBuilder()  
    .withUrl(info.endpoint, options)  
    .configureLogging(signalR.LogLevel.Information)  
    .build();  
  
    connection.on('broadcastData', (message) => {  
        new Notification(message, {  
            icon: '48.png',  
            body: message  
          });     
  });  
  connection.onclose(() => console.log('disconnected'));  
  
  console.log('connecting...');  
  connection.start()  
    .then(() => console.log('connected!'))  
    .catch(console.error);  
}).catch(alert);  
  
function getConnectionInfo() {  
  return axios.post(`${apiBaseUrl}/api/negotiate`)  
    .then(resp => resp.data);  
}  
```

# Conclusion
With a few lines of code, we have created the serverless Azure functions, which will fetch the data from the API endpoint and broadcast the messages to all connected clients in real time using Azure SignalR. In this article, I have hard coded the API key in the program but ideally, it should be stored in Azure Key Vault and read it from there.
 
