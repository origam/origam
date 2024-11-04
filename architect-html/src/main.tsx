import React, { createContext } from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { store } from "src/stores/store.ts";
import { Provider } from "react-redux";
import { RootStore } from "src/stores/RootStore.ts";
import { ArchitectApi } from "src/API/ArchitectApi.ts";
import { UiStore } from "src/stores/UiStore.ts";


const architectApi = new ArchitectApi();
const rootSore = new RootStore(architectApi)
const uiStore = new UiStore();

export const RootStoreContext = createContext(rootSore);
export const UiStoreContext = createContext(uiStore);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <Provider store={store}>
      <RootStoreContext.Provider value={rootSore}>
        <UiStoreContext.Provider value={uiStore}>
          <App/>
        </UiStoreContext.Provider>
      </RootStoreContext.Provider>
    </Provider>
  </React.StrictMode>
)
