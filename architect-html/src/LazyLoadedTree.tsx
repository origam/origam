import React, { useState } from 'react';

export interface TreeNode {
  id: string;
  nodeText: string;
  hasChildNodes: boolean;
  isNonPersistentItem: boolean;
  children?: TreeNode[];
  isLoading?: boolean;
}

export interface TreeProps {
  data: TreeNode[];
  onLoadChildren: (node: TreeNode) => Promise<TreeNode[]>;
}

const TreeNodeComponent: React.FC<{
  node: TreeNode;
  onLoadChildren: (node: TreeNode) => Promise<TreeNode[]>;
}> = ({ node, onLoadChildren }) => {

  const [isExpanded, setIsExpanded] = useState(false)
  function onToggle(){
    setIsExpanded(!isExpanded);
  }

  const handleToggle = async () => {
    if (!node.children && node.hasChildNodes && !node.isLoading) {
      node.children = await onLoadChildren(node);
      onToggle();
    } else {
      onToggle();
    }
  };

  return (
    <div style={{ marginLeft: '20px' }}>
      <div onClick={handleToggle} style={{ cursor: 'pointer' }}>
        {node.children ? (isExpanded ? '▼' : '▶') : '•'} {node.nodeText}
        {node.isLoading && ' Loading...'}
      </div>
      {isExpanded && node.children && (
        <div>
          {node.children.map((childNode) => (
            <TreeNodeComponent
              key={childNode.id}
              node={childNode}
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
  onLoadChildren: (node: TreeNode) => Promise<TreeNode[]>;
}> = ({ topNodes, onLoadChildren }) => {
  return (
    <div>
      {topNodes.map((node) => (
        <TreeNodeComponent
          key={node.id}
          node={node}
          onLoadChildren={onLoadChildren}
        />
      ))}
    </div>
  );
};

export default LazyLoadedTree;