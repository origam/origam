import React, { useEffect, useState } from 'react';
import LazyLoadedTree, { TreeNode } from 'src/components/lazyLoadedTree/LazyLoadedTree.tsx';
import axios from "axios";
import { Packages } from "src/components/packages/Packages.tsx";
import { GridEditor } from "src/components/gridEditor/GridEditor.tsx";
import "./App.css"
import "src/colors.scss"

const App: React.FC = () => {
  const [page, setPage] = useState<CurrentPage>(CurrentPage.Packages)
  const [topNodes, setTopNodes] = useState<TreeNode[]>([])
  const [editorNode, setEditorNode] = useState<TreeNode | undefined>()

  const loadChildren = async (node: TreeNode): Promise<TreeNode[]> => {
    return (await axios.get(
      `/Model/GetChildren`,
      {
        params: {
          id: node.id,
          nodeText: node.nodeText,
          isNonPersistentItem: node.isNonPersistentItem
        }
      })).data;
  };

  async function loadTopNodes() {
    setTopNodes((await axios.get(`/Model/GetTopNodes`)).data);
  }

  async function onPackageLoaded() {
    await loadTopNodes();
    setPage(CurrentPage.Model)
  }

  useEffect(() => {
    loadTopNodes();
  }, []);

  return (
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
          onLoadChildren={loadChildren}/>}
      {page === CurrentPage.Editor &&
        <GridEditor
          node={editorNode!}
          onBackClick={() => setPage(CurrentPage.Model)}
        />}
    </div>
  );
};

enum CurrentPage {
  Packages = "packages",
  Model = "model",
  Editor = "editor"
}

export default App;