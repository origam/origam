import React, { createContext } from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { RootStore } from "src/stores/RootStore.ts";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import { getLocaleFromCookie, initLocaleCookie } from "src/utils/cookie.ts";


const rootStore = new RootStore();
export const RootStoreContext = createContext(rootStore);

const root = ReactDOM.createRoot(document.getElementById('root')!);

export function T(defaultContent: any, translKey: string, ...p: any[]) {
  return rootStore.translations.T(defaultContent, translKey, ...p);
}

async function main() {
  const run = runInFlowWithHandler(rootStore.errorDialogController)
  initLocaleCookie();
  const locale = getLocaleFromCookie();
  await run({generator: rootStore.translations.setLocale(locale)});

  root.render(
    <React.StrictMode>
      <RootStoreContext.Provider value={rootStore}>
        <App/>
      </RootStoreContext.Provider>
    </React.StrictMode>
  )
}

main();