import { useEffect } from 'react';
import './App.scss'
import Header from './components/Header/Header'
import { start, stop } from './signalr-context';
import Footer from './components/Footer/Footer';
import { Route, Routes } from 'react-router-dom';
import DesignPage from './pages/DesignPage/DesignPage';
import DemoPage from './pages/DemoPage/DemoPage';

function App() {

  useEffect(() => {
    start();
    return () => {
      stop();
    };
  }, []);

  return (
    <>
      <Header />
      <Routes>
        <Route path="/" element={<DesignPage />} />
        <Route path="/demo" element={<DemoPage />} />
        <Route path="/design" element={<DesignPage />} />
      </Routes>
      <Footer />
    </>
  )
}

export default App
