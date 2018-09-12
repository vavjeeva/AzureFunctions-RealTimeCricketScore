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
