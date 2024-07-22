import React, { useContext, useState } from 'react';
import "src/components/lazyLoadedTree/LazyLoadedTree.css"
import { ArchitectApiContext } from "src/API/ArchitectApiContext.tsx";

export interface TreeNode {
  id: string;
  nodeText: string;
  hasChildNodes: boolean;
  isNonPersistentItem: boolean;
  editorType: null | "GridEditor";
  children?: TreeNode[];
  isLoading?: boolean;
}

const TreeNodeComponent: React.FC<{
  node: TreeNode;
  openEditor: (node: TreeNode) => void;
}> = ({node, openEditor}) => {
  const architectApi = useContext(ArchitectApiContext)!;
  const [isExpanded, setIsExpanded] = useState(false)

  const onNodeDoubleClick = async (node: TreeNode) => {
    if (!node.editorType) {
      await onToggle();
    } else {
      openEditor(node);
    }
  }

  const onToggle = async () => {
    if (!node.children && node.hasChildNodes && !node.isLoading) {
      node.children = await architectApi.getNodeChildren(node);
    }
    setIsExpanded(!isExpanded);
  };

  return (
    <div className={"treeNode"}>
      <div className={"treeNodeTitle"}>
        <div onClick={onToggle}>{node.hasChildNodes ? (isExpanded ? '▼' : '▶') : '•'}</div>
        <div onDoubleClick={() => onNodeDoubleClick(node)}>{node.nodeText}</div>
        {node.isLoading && ' Loading...'}
      </div>
      {isExpanded && node.children && (
        <div>
          {node.children.map((childNode) => (
            <TreeNodeComponent
              key={childNode.id + childNode.nodeText}
              node={childNode}
              openEditor={openEditor}
            />
          ))}
        </div>
      )}
    </div>
  );
};

const LazyLoadedTree: React.FC<{
  topNodes: TreeNode[];
  openEditor: (node: TreeNode) => void;
}> = ({topNodes, openEditor}) => {
  return (
    <div>
      {topNodes.map((node) => (
        <TreeNodeComponent
          key={node.id}
          node={node}
          openEditor={openEditor}
        />
      ))}
    </div>
  );
};

export default LazyLoadedTree;