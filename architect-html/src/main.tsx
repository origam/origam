import React, { createContext } from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { RootStore } from "src/stores/RootStore.ts";
import { UiStore } from "src/stores/UiStore.ts";

const uiStore = new UiStore();
const rootStore = new RootStore(uiStore);
export const RootStoreContext = createContext(rootStore);
export const UiStoreContext = createContext(uiStore);
const root = ReactDOM.createRoot(document.getElementById('root')!);
root.render(
  <React.StrictMode>
    <RootStoreContext.Provider value={rootStore}>
      <UiStoreContext.Provider value={uiStore}>
        <App/>
      </UiStoreContext.Provider>
    </RootStoreContext.Provider>
  </React.StrictMode>
)