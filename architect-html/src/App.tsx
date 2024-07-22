import React, { useEffect, useState } from 'react';
import LazyLoadedTree, { TreeNode } from 'src/components/lazyLoadedTree/LazyLoadedTree.tsx';
import { Packages } from "src/components/packages/Packages.tsx";
import { GridEditor } from "src/components/gridEditor/GridEditor.tsx";
import "./App.css"
import "src/colors.scss"
import { ArchitectApiProvider } from "src/API/ArchitectApiContext.tsx";
import { ArchitectApi } from "src/API/ArchitectApi.ts";

const App: React.FC = () => {
  const [page, setPage] = useState<CurrentPage>(CurrentPage.Packages)
  const [topNodes, setTopNodes] = useState<TreeNode[]>([])
  const [editorNode, setEditorNode] = useState<TreeNode | undefined>()

  const architectApi = new ArchitectApi();
  async function loadTopNodes() {
    setTopNodes(await architectApi.getTopModelNodes());
  }

  async function onPackageLoaded() {
    await loadTopNodes();
    setPage(CurrentPage.Model)
  }

  useEffect(() => {
    loadTopNodes();
  }, []);

  return (
    <ArchitectApiProvider api={architectApi}>
      <div>
        {page === CurrentPage.Packages &&
          <Packages onPackageLoaded={onPackageLoaded}/>}
        {page === CurrentPage.Model &&
          <LazyLoadedTree
            topNodes={topNodes}
            openEditor={(node) => {
              setEditorNode(node);
              setPage(CurrentPage.Editor);
            }}
          />}
        {page === CurrentPage.Editor &&
          <GridEditor
            node={editorNode!}
            onBackClick={() => setPage(CurrentPage.Model)}
          />}
      </div>
    </ArchitectApiProvider>
  );
};

enum CurrentPage {
  Packages = "packages",
  Model = "model",
  Editor = "editor"
}

export default App;