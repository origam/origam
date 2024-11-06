import { useContext, useEffect } from 'react';
import LazyLoadedTree from 'src/components/lazyLoadedTree/LazyLoadedTree.tsx';
import { Packages } from "src/components/packages/Packages.tsx";
import "./App.css"
import "src/colors.scss"
import { TopLayout } from "src/components/topLayout/TopLayout.tsx";
import { TabView } from "src/components/tabView/TabView.tsx";
import { SaveButton } from "src/components/saveButton/SaveButton.tsx";
import { RootStoreContext, UiStoreContext } from "src/main.tsx";
import { flow } from "mobx";
import { observer } from "mobx-react-lite";
import { EditorTabView } from "src/components/editorTabView/EditorTabView.tsx";

const App: React.FC = observer(() => {

  const rootStore = useContext(RootStoreContext);
  const uiStore = useContext(UiStoreContext);

  useEffect(() => {
    flow(rootStore.projectState.loadPackageNodes.bind(rootStore.projectState))();
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
          state={uiStore.sideBarTabViewState}
          items={[
            {
              label: "Packages",
              node: <Packages/>
            },
            {
              label: "Model",
              node: <LazyLoadedTree/>
            }
          ]}
        />
      }
    />
  );
});

export default App;