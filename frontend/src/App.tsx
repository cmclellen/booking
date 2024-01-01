import { useEffect, useState } from 'react';
import './App.scss'
import Header from './components/Header/Header'
import { createHubConnection } from './signalr-context';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';

function App() {

  const [connectionRef, setConnection] = useState<HubConnection>();

  useEffect(() => {
    if(!connectionRef) {
      setConnection(createHubConnection());
    }
  }, []);

  function start(): any {
    if (connectionRef) {
      if(connectionRef.state !== HubConnectionState.Disconnected) return;
      console.log(`SignalR connecting [${connectionRef.state}]...`);
      connectionRef
        .start()
        .then(() => {
          console.log('SignalR connected.')
        })
        .catch((err: Error) => {
          console.error(err);
          return setTimeout(start, 3000);
        });
    }
  }

  useEffect(() => {
    const timer = start();

    return () => {
      if(timer) clearTimeout(timer);
      connectionRef?.stop();
    };
  }, [connectionRef]);

  return (
    <Header />
  )
}

export default App
