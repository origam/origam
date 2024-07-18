import React from 'react';

export interface TreeNode {
  id: string;
  title: string;
  isLeaf: boolean;
  children?: TreeNode[];
  isLoading?: boolean;
  isExpanded?: boolean;
}

export interface TreeProps {
  data: TreeNode[];
  onLoadChildren: (node: TreeNode) => Promise<TreeNode[]>;
}

const TreeNodeComponent: React.FC<{
  node: TreeNode;
  onLoadChildren: (node: TreeNode) => Promise<TreeNode[]>;
}> = ({ node, onLoadChildren }) => {

  function onToggle(node: TreeNode){
    node.isExpanded = !node.isExpanded;
  }

  const handleToggle = async () => {
    if (!node.children && !node.isLeaf && !node.isLoading) {
      node.children = await onLoadChildren(node);
      onToggle(node);
    } else {
      onToggle(node);
    }
  };

  return (
    <div style={{ marginLeft: '20px' }}>
      <div onClick={handleToggle} style={{ cursor: 'pointer' }}>
        {node.children ? (node.isExpanded ? '▼' : '▶') : '•'} {node.title}
        {node.isLoading && ' Loading...'}
      </div>
      {node.isExpanded && node.children && (
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
  // const [topNodes, setTopNodes] = useState<TreeNode[]>(data);

  // const handleToggle = useCallback((toggledNode: TreeNode) => {
  //   setTopNodes((prevData) =>
  //     updateTreeData(prevData, toggledNode.id, toggledNode)
  //   );
  // }, []);
  //
  // const updateTreeData = (
  //   list: TreeNode[],
  //   id: string,
  //   updatedNode: TreeNode
  // ): TreeNode[] => {
  //   return list.map((node) => {
  //     if (node.id === id) {
  //       return updatedNode;
  //     }
  //     if (node.children) {
  //       return {
  //         ...node,
  //         children: updateTreeData(node.children, id, updatedNode),
  //       };
  //     }
  //     return node;
  //   });
  // };

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