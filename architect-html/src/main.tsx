/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import App from '@/App.tsx';
import '@/index.css';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler.ts';
import { RootStore } from '@stores/RootStore.ts';
import { getLocaleFromCookie, initLocaleCookie } from '@utils/cookie.ts';
import React, { createContext } from 'react';
import ReactDOM from 'react-dom/client';

const rootStore = new RootStore();
export const RootStoreContext = createContext(rootStore);

const root = ReactDOM.createRoot(document.getElementById('root')!);

export function T(defaultContent: any, translKey: string, ...p: any[]) {
  return rootStore.translations.T(defaultContent, translKey, ...p);
}

async function main() {
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  initLocaleCookie();
  const locale = getLocaleFromCookie();
  await run({ generator: rootStore.translations.setLocale(locale) });

  root.render(
    <React.StrictMode>
      <RootStoreContext.Provider value={rootStore}>
        <App />
      </RootStoreContext.Provider>
    </React.StrictMode>,
  );
}

main();
