import React, { createContext } from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { RootStore } from "src/stores/RootStore.ts";
import { UiStore } from "src/stores/UiStore.ts";

const uiStore = new UiStore();
const rootSore = new RootStore(uiStore);

export const RootStoreContext = createContext(rootSore);
export const UiStoreContext = createContext(uiStore);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <RootStoreContext.Provider value={rootSore}>
      <UiStoreContext.Provider value={uiStore}>
        <App/>
      </UiStoreContext.Provider>
    </RootStoreContext.Provider>
  </React.StrictMode>
)
