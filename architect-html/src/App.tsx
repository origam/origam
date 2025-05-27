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
                label: T("app_packages", "appPackages"),
                node: <Packages/>
              },
              {
                label: "Model",
                node: <ModelTree/>
              },
              {
                label: "Properties",
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