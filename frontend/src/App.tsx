import { useEffect } from 'react';
import './App.scss'
import Header from './components/Header/Header'
import { start, stop } from './signalr-context';

function App() {

  useEffect(() => {
    start();
    return () => {
      stop();
    };
  }, []);

  return (
    <Header />
  )
}

export default App
