import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'
import './index.css'

const isLocalPreview = ['127.0.0.1', 'localhost'].includes(window.location.hostname)

if ('serviceWorker' in navigator && isLocalPreview) {
  navigator.serviceWorker.getRegistrations().then((registrations) => {
    registrations.forEach((registration) => {
      registration.unregister().catch(() => undefined)
    })
  })
}

if ('serviceWorker' in navigator && !isLocalPreview) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js').catch(() => undefined)
  })
}

ReactDOM.createRoot(document.getElementById('root') as HTMLElement).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)
