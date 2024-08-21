import React, { ReactNode, useEffect, useState } from 'react';
import LazyLoadedTree, { TreeNode } from 'src/components/lazyLoadedTree/LazyLoadedTree.tsx';
import { Packages } from "src/components/packages/Packages.tsx";
import { GridEditor } from "src/components/editors/gridEditor/GridEditor.tsx";
import "./App.css"
import "src/colors.scss"
import { ArchitectApiProvider } from "src/API/ArchitectApiContext.tsx";
import { ArchitectApi } from "src/API/ArchitectApi.ts";
import { TopLayout } from "src/components/topLayout/TopLayout.tsx";
import { TabView, TabViewId } from "src/components/tabView/TabView.tsx";
import { SaveButton } from "src/components/saveButton/SaveButton.tsx";
import { XsltEditor } from "src/components/editors/xsltEditor/XsltEditor.tsx";
import {
  ScreenSectionEditor
} from "src/components/screenSectionEditor2/ScreenSectionEditor.tsx";

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
      <ScreenSectionEditor/>
      {/*<TopLayout*/}
      {/*  topToolBar={<SaveButton/>}*/}
      {/*  editorArea={editor}*/}
      {/*  sideBar={*/}
      {/*    <TabView items={[*/}
      {/*      {*/}
      {/*        id: TabViewId.Packages,*/}
      {/*        label: "Packages",*/}
      {/*        node: <Packages onPackageLoaded={onPackageLoaded}/>*/}
      {/*      },*/}
      {/*      {*/}
      {/*        id: TabViewId.Model,*/}
      {/*        label: "Model",*/}
      {/*        node: <LazyLoadedTree*/}
      {/*          topNodes={topNodes}*/}
      {/*          openEditor={(node) => {*/}
      {/*            setEditor(getEditor(node));*/}
      {/*          }}*/}
      {/*        />*/}
      {/*      }*/}
      {/*    ]}/>*/}
      {/*  }*/}
      {/*/>*/}
    </ArchitectApiProvider>
  );
};

function getEditor(node: TreeNode){
  if(node.editorType === "GridEditor") {
    return <GridEditor node={node}/>
  }
  if(node.editorType === "XslTEditor") {
    return <XsltEditor node={node}/>
  }
}

export default App;