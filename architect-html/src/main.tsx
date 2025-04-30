import React, { createContext } from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { RootStore } from "src/stores/RootStore.ts";


const rootStore = new RootStore();
export const RootStoreContext = createContext(rootStore);

const root = ReactDOM.createRoot(document.getElementById('root')!);
root.render(
  <React.StrictMode>
    <RootStoreContext.Provider value={rootStore}>
      <App/>
    </RootStoreContext.Provider>
  </React.StrictMode>
)