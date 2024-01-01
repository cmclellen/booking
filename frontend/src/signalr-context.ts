import * as signalR from '@microsoft/signalr';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

const hubUrl = `${apiBaseUrl}/api`;
console.log(`hub url: ${hubUrl}`);

export function createHubConnection() {
  console.log('Creating SignalR HubConnection...');
  const connection: signalR.HubConnection = new signalR.HubConnectionBuilder()
    .withAutomaticReconnect()  
    .withUrl(hubUrl)  
    .configureLogging(signalR.LogLevel.Information)
    .build();

  connection.onclose(() => console.log('SignalR disconnected.'));

  return connection;
}
