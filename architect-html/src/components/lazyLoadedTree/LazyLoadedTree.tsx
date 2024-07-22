import React, { useState } from 'react';
import "src/components/lazyLoadedTree/LazyLoadedTree.css"

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
  onLoadChildren: (node: TreeNode) => Promise<TreeNode[]>;
}> = ({node, onLoadChildren, openEditor}) => {

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
      node.children = await onLoadChildren(node);
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
              onLoadChildren={onLoadChildren}
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
  onLoadChildren: (node: TreeNode) => Promise<TreeNode[]>;
}> = ({topNodes, onLoadChildren, openEditor}) => {
  return (
    <div>
      {topNodes.map((node) => (
        <TreeNodeComponent
          key={node.id}
          node={node}
          openEditor={openEditor}
          onLoadChildren={onLoadChildren}
        />
      ))}
    </div>
  );
};

export default LazyLoadedTree;