import React, { ReactNode, useEffect, useState } from 'react';
import LazyLoadedTree, { TreeNode } from 'src/components/lazyLoadedTree/LazyLoadedTree.tsx';
import { Packages } from "src/components/packages/Packages.tsx";
import { GridEditor } from "src/components/gridEditor/GridEditor.tsx";
import "./App.css"
import "src/colors.scss"
import { ArchitectApiProvider } from "src/API/ArchitectApiContext.tsx";
import { ArchitectApi } from "src/API/ArchitectApi.ts";
import { TopLayout } from "src/components/topLayout/TopLayout.tsx";
import { TabView, TabViewId } from "src/components/tabView/TabView.tsx";

const App: React.FC = () => {
  const [editor, setEditor] = useState<ReactNode | undefined>()
  const [topNodes, setTopNodes] = useState<TreeNode[]>([])

  const architectApi = new ArchitectApi();

  async function loadTopNodes() {
    setTopNodes(await architectApi.getTopModelNodes());
  }

  async function onPackageLoaded() {
    await loadTopNodes();
  }

  useEffect(() => {
    loadTopNodes();
  }, []);

  return (
    <ArchitectApiProvider api={architectApi}>
      <TopLayout
        topToolBar={<div/>}
        editorArea={editor}
        sideBar={
          <TabView items={[
            {
              id: TabViewId.Packages,
              label: "Packages",
              node: <Packages onPackageLoaded={onPackageLoaded}/>
            },
            {
              id: TabViewId.Model,
              label: "Model",
              node: <LazyLoadedTree
                topNodes={topNodes}
                openEditor={(node) => {
                  setEditor(getEditor(node));
                }}
              />
            }
          ]}/>
        }
      />
    </ArchitectApiProvider>
  );
};

function getEditor(node: TreeNode){
  return(
      <GridEditor node={node}/>
  );
}

export default App;