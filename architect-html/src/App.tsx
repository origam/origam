import React, { useEffect, useState } from 'react';
import LazyLoadedTree, { TreeNode } from './LazyLoadedTree';
import axios from "axios";
import { Packages } from "./Packages.tsx";

const App: React.FC = () => {
  const [topNodes, setTopNodes] = useState<TreeNode[]>([])

  const loadChildren = async (node: TreeNode): Promise<TreeNode[]> => {
    return (await axios.get(
      `/Model/GetChildren`,
      { params: {
        id: node.id,
        nodeText: node.nodeText,
        isNonPersistentItem: node.isNonPersistentItem
      } })).data;
  };

  async function loadTopNodes() {
    setTopNodes((await axios.get(`/Model/GetTopNodes`)).data);
  }

  useEffect(() => {
    loadTopNodes();
  }, []);

  return (
    <div>
      <Packages onPackageLoaded={loadTopNodes}/>
      <h3>Model</h3>
      <LazyLoadedTree topNodes={topNodes} onLoadChildren={loadChildren}/>
    </div>
);
};

export default App;