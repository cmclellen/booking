import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.scss'
// import '../node_modules/bootstrap/dist/js/bootstrap.js';
import { HashRouter, Route, Routes } from 'react-router-dom';
import HomePage from './pages/HomePage/HomePage.tsx';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
console.log(`API base url: ${apiBaseUrl}`);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <HashRouter basename='/'>
      <App />
      <Routes>
        <Route path="/" element={<HomePage />} />
      </Routes>
    </HashRouter>
  </React.StrictMode>,
)
