import * as signalR from '@microsoft/signalr';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

const hubUrl = `${apiBaseUrl}/api`;
console.log(`hub url: ${hubUrl}`);

const connection = new signalR.HubConnectionBuilder()
  .withAutomaticReconnect()  
  .withUrl(hubUrl)  
  .configureLogging(signalR.LogLevel.Information)
  .build();

connection.onclose(() => console.log('SignalR disconnected.'));

async function start() {
  try {
    console.log('SignalR connecting...');
    await connection.start();
    console.log('SignalR connected.');
  } catch(err) {
    console.error(err);
    setTimeout(start, 4000)
  }
}

await start();

export default connection;
