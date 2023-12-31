import * as signalR from '@microsoft/signalr';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

const hubUrl = `${apiBaseUrl}/api`;
console.log(`hub url: ${hubUrl}`);
const connection = new signalR.HubConnectionBuilder()
  .withUrl(hubUrl)
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();

connection.onclose(() => console.log('disconnected'));

console.log('connecting...');
connection.start()
  .then(() => console.log('connected'))
  .catch(console.error);

export default connection;
