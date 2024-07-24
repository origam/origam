import React, { useContext, useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import "src/components/lazyLoadedTree/LazyLoadedTree.css"
import { ArchitectApiContext } from "src/API/ArchitectApiContext.tsx";
import {
  toggleNode,
  selectExpandedNodes
} from 'src/components/lazyLoadedTree/LazyLoadedTreeSlice.ts';

export interface TreeNode {
  id: string;
  nodeText: string;
  hasChildNodes: boolean;
  isNonPersistentItem: boolean;
  editorType: null | "GridEditor";
  children?: TreeNode[];
}

const TreeNodeComponent: React.FC<{
  node: TreeNode;
  openEditor: (node: TreeNode) => void;
  children?: TreeNode[];
}> = ({node, openEditor, children}) => {
  const architectApi = useContext(ArchitectApiContext)!;
  const dispatch = useDispatch();
  const expandedNodes = useSelector(selectExpandedNodes);
  const isExpanded = expandedNodes.includes(node.id);
  const[childNodes, setChildNodes] = useState(children)
  const [isLoading, setIsLoading] = useState(false)

  useEffect(() => {
    if (isExpanded && node.hasChildNodes) {
      loadChildren();
    }
  }, [isExpanded, node.hasChildNodes]);

  async function loadChildren() {
    if (!childNodes && !isLoading) {
      setIsLoading(true);
      try {
        const nodes = await architectApi.getNodeChildren(node);
        setChildNodes(nodes);
      } finally {
        setIsLoading(false);
      }
    }
  }

  const onNodeDoubleClick = async (node: TreeNode) => {
    if (!node.editorType) {
      await onToggle();
    } else {
      openEditor(node);
    }
  }

  const onToggle = async () => {
    if (node.hasChildNodes && !isLoading) {
      await loadChildren();
    }
    dispatch(toggleNode(node.id));
  };

  return (
    <div className={"treeNode"}>
      <div className={"treeNodeTitle"}>
        <div onClick={onToggle}>
          {node.hasChildNodes ? (isExpanded ? '▼' : '▶') : '•'}
        </div>
        <div onDoubleClick={() => onNodeDoubleClick(node)}>{node.nodeText}</div>
        {isLoading && ' Loading...'}
      </div>
      {isExpanded && childNodes && (
        <div>
          {childNodes.map((childNode) => (
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
          children={node.children} // The top nodes come with preloaded children
        />
      ))}
    </div>
  );
};

export default LazyLoadedTree;