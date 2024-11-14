import { useContext, useEffect } from 'react';
import { Packages } from "src/components/packages/Packages.tsx";
import "./App.css"
import "src/colors.scss"
import { TopLayout } from "src/components/topLayout/TopLayout.tsx";
import { TabView } from "src/components/tabView/TabView.tsx";
import { RootStoreContext } from "src/main.tsx";
import { observer } from "mobx-react-lite";
import { EditorTabView } from "src/components/editorTabView/EditorTabView.tsx";
import ModelTree from "src/components/modelTree/ModelTree.tsx";
import { TopBar } from "src/components/topBar/TopBar.tsx";
import { ApplicationDialogStack } from "src/dialog/DialogStack.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import ComponentDesigner
  from "src/components/editors/screenSectionEditor/ComponentDesigner.tsx";

const App: React.FC = observer(() => {

  const rootStore = useContext(RootStoreContext);

  useEffect(() => {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: rootStore.packagesState.loadPackages.bind(rootStore.modelTreeState),
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
      {/*<ComponentDesigner/>*/}
      <TopLayout
        topToolBar={<TopBar/>}
        editorArea={<EditorTabView/>}
        sideBar={
          <TabView
            state={rootStore.sideBarTabViewState}
            items={[
              {
                label: "Packages",
                node: <Packages/>
              },
              {
                label: "Model",
                node: <ModelTree/>
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