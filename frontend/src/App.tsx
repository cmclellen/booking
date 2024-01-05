import { useEffect } from 'react';
import './App.scss'
import Header from './components/Header/Header'
import Footer from './components/Footer/Footer';
import { Route, Routes } from 'react-router-dom';
import DesignPage from './pages/DesignPage/DesignPage';
import DemoPage from './pages/DemoPage/DemoPage';
import { signalRState } from './signalr-context';

function App() {

  useEffect(() => {
    signalRState.start();
    return () => {
      signalRState.stop();
    };
  }, []);

  return (
    <>
      {/* <SignalRStateContext.Provider value={signalRState}> */}
        <Header />
        <Routes>
          <Route path="/" element={<DesignPage />} />
          <Route path="/demo" element={<DemoPage />} />
          <Route path="/design" element={<DesignPage />} />
        </Routes>
        <Footer />
      {/* </SignalRStateContext.Provider> */}
    </>
  )
}

export default App
