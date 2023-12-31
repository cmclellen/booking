import './App.scss'
import * as signalR from '@microsoft/signalr';
import Header from './components/Header/Header'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

const hubUrl = `${apiBaseUrl}/api`;
console.log(`hub url: ${hubUrl}`);
const connection = new signalR.HubConnectionBuilder()
  .withUrl(hubUrl)
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();

connection.on('FlightBookedEvent', (message) => {
  console.log(`message: ${message}`);
});

connection.onclose(() => console.log('disconnected'));

console.log('connecting...');
connection.start()
  .then(() => console.log('connected'))
  .catch(console.error);

function App() {

  return (    
      <Header />
  )
}

export default App
