import * as signalR from '@microsoft/signalr';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

const hubUrl = `${apiBaseUrl}/api`;
console.log(`hub url: ${hubUrl}`);

export function createHubConnection(): signalR.HubConnection {
  console.log('Creating SignalR HubConnection...');
  const connection: signalR.HubConnection = new signalR.HubConnectionBuilder()
    .withAutomaticReconnect()  
    .withUrl(hubUrl)  
    .configureLogging(signalR.LogLevel.Information)
    .build();

  connection.onclose(() => console.log('SignalR disconnected.'));

  return connection;
}

var timeout: any = undefined;

export function start(): any {
  if (connectionRef) {
    if(connectionRef.state !== signalR.HubConnectionState.Disconnected) return;
    console.log(`SignalR connecting [${connectionRef.state}]...`);
    connectionRef
      .start()
      .then(() => console.log('SignalR connected.'))
      .catch((err: Error) => {
        console.error(err);
        timeout = setTimeout(start, 3000);
      });
  }
}

export function stop() {
  if(timeout) clearTimeout(timeout);
  connectionRef?.stop();
}

var connectionRef: signalR.HubConnection | undefined = undefined;
if(!connectionRef) {
  connectionRef = createHubConnection();
}

export default connectionRef;
