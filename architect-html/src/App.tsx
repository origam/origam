import { ReactNode, useContext, useEffect, useState } from 'react';
import LazyLoadedTree from 'src/components/lazyLoadedTree/LazyLoadedTree.tsx';
import { Packages } from "src/components/packages/Packages.tsx";
import "./App.css"
import "src/colors.scss"
import { ArchitectApiProvider } from "src/API/ArchitectApiContext.tsx";
import { ArchitectApi } from "src/API/ArchitectApi.ts";
import { TopLayout } from "src/components/topLayout/TopLayout.tsx";
import { TabView } from "src/components/tabView/TabView.tsx";
import { SaveButton } from "src/components/saveButton/SaveButton.tsx";
import { RootStoreContext, UiStoreContext } from "src/main.tsx";
import { flow } from "mobx";
import { observer } from "mobx-react-lite";

const App: React.FC =  observer(() => {
  const [editor, setEditor] = useState<ReactNode | undefined>()
  const architectApi = new ArchitectApi();
  const rootStore = useContext(RootStoreContext);
  const uiStore = useContext(UiStoreContext);

  useEffect(() => {
    flow(rootStore.projectState.loadPackageNodes.bind(rootStore.projectState))();
  }, []);

  return (
    <ArchitectApiProvider api={architectApi}>
      {/*<ScreenSectionEditor/>*/}
      <TopLayout
        topToolBar={<SaveButton/>}
        editorArea={rootStore.projectState.activeEditor}
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
    </ArchitectApiProvider>
  );
});

export default App;