import { useContext, useEffect } from 'react';
import { Packages } from "src/components/packages/Packages.tsx";
import "./App.css"
import "src/colors.scss"
import { TopLayout } from "src/components/topLayout/TopLayout.tsx";
import { TabView } from "src/components/tabView/TabView.tsx";
import { SaveButton } from "src/components/saveButton/SaveButton.tsx";
import { RootStoreContext } from "src/main.tsx";
import { flow } from "mobx";
import { observer } from "mobx-react-lite";
import { EditorTabView } from "src/components/editorTabView/EditorTabView.tsx";
import ModelTree from "src/components/modelTree/ModelTree.tsx";

const App: React.FC = observer(() => {

  const rootStore = useContext(RootStoreContext);

  useEffect(() => {
    flow(rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState))();
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

  // {/*<ScreenSectionEditor/>*/}
  return (
    <TopLayout
      topToolBar={<SaveButton/>}
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
  );
});

export default App;