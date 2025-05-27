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

import { useContext, useEffect } from 'react';
import { Packages } from "src/components/packages/Packages.tsx";
import "./App.css"
import "src/colors.scss"
import { TopLayout } from "src/components/topLayout/TopLayout.tsx";
import { TabView } from "src/components/tabView/TabView.tsx";
import { RootStoreContext, T } from "src/main.tsx";
import { observer } from "mobx-react-lite";
import { EditorTabView } from "src/components/editorTabView/EditorTabView.tsx";
import ModelTree from "src/components/modelTree/ModelTree.tsx";
import { TopBar } from "src/components/topBar/TopBar.tsx";
import { ApplicationDialogStack } from "src/dialog/DialogStack.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import { Properties } from "src/components/properties/Properties.tsx";

const App: React.FC = observer(() => {

  const rootStore = useContext(RootStoreContext);

  useEffect(() => {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: rootStore.packagesState.loadPackages.bind(rootStore.packagesState),
    });
  }, []);

  useEffect(() => {
    const handleContextMenu = (e: MouseEvent) => {
      e.preventDefault();
      return false;
    };
    document.addEventListener('contextmenu', handleContextMenu);
    return () => {
      document.removeEventListener('contextmenu', handleContextMenu);
    };
  }, []);

  return (
    <>
      <TopLayout
        topToolBar={<TopBar/>}
        editorArea={<EditorTabView/>}
        sideBar={
          <TabView
            width={400}
            state={rootStore.sideBarTabViewState}
            items={[
              {
                label: T("Packages", "app_packages"),
                node: <Packages/>
              },
              {
                label: T("Model", "app_model"),
                node: <ModelTree/>
              },
              {
                label: T("Properties", "app_properties"),
                node: <Properties/>
              }
            ]}
          />
        }
      />
      <ApplicationDialogStack/>
    </>
  );
});

export default App;