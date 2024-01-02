import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.scss'
import '../node_modules/bootstrap/dist/js/bootstrap.js';
import { HashRouter } from 'react-router-dom';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
console.log(`API base url: ${apiBaseUrl}`);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <HashRouter basename='/'>
      <App />
    </HashRouter>
  </React.StrictMode>,
)
