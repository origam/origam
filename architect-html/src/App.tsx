import { ReactNode, useContext, useEffect, useState } from 'react';
import LazyLoadedTree, {
} from 'src/components/lazyLoadedTree/LazyLoadedTree.tsx';
import { Packages } from "src/components/packages/Packages.tsx";
import { GridEditor } from "src/components/editors/gridEditor/GridEditor.tsx";
import "./App.css"
import "src/colors.scss"
import { ArchitectApiProvider } from "src/API/ArchitectApiContext.tsx";
import { ArchitectApi } from "src/API/ArchitectApi.ts";
import { TopLayout } from "src/components/topLayout/TopLayout.tsx";
import { TabView } from "src/components/tabView/TabView.tsx";
import { SaveButton } from "src/components/saveButton/SaveButton.tsx";
import { XsltEditor } from "src/components/editors/xsltEditor/XsltEditor.tsx";
import { RootStoreContext, UiStoreContext } from "src/main.tsx";
import { flow } from "mobx";

import { TreeNode } from "src/stores/TreeNode.ts";

const App: React.FC = () => {
  const [editor, setEditor] = useState<ReactNode | undefined>()
  const architectApi = new ArchitectApi();
  const rootStore = useContext(RootStoreContext);
  const uiStore = useContext(UiStoreContext);

  // async function loadTopNodes() {
  //   const topNodes= await architectApi.getTopModelNodes();
  //   dispatch(addTopNodes(topNodes));
  // }
  //
  // async function onPackageLoaded() {
  //   await loadTopNodes();
  //
  // }

  useEffect(() => {
    flow(rootStore.projectState.loadPackageNodes.bind(rootStore.projectState))();
  }, []);

  return (
    <ArchitectApiProvider api={architectApi}>
      {/*<ScreenSectionEditor/>*/}
      <TopLayout
        topToolBar={<SaveButton/>}
        editorArea={editor}
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
                node: <LazyLoadedTree
                  openEditor={(node) => {
                    setEditor(getEditor(node));
                  }}
                />
              }
            ]}
          />
        }
      />
    </ArchitectApiProvider>
  );
};

function getEditor(node: TreeNode) {
  if (node.editorType === "GridEditor") {
    return <GridEditor node={node}/>
  }
  if (node.editorType === "XslTEditor") {
    return <XsltEditor node={node}/>
  }
}

export default App;