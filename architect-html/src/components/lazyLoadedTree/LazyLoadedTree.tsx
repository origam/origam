import { useContext, useEffect, useMemo, useState } from 'react';
import { useSelector, useDispatch, shallowEqual } from 'react-redux';
import "src/components/lazyLoadedTree/LazyLoadedTree.css"
import { ArchitectApiContext } from "src/API/ArchitectApiContext.tsx";
import {
  toggleNode,
  selectExpandedNodes,
  TreeNode,
  selectTopNodes,
  setChildNodes, SelectChildNodes
} from 'src/components/lazyLoadedTree/LazyLoadedTreeSlice.ts';
import {
  Menu,
  Item,
  useContextMenu, TriggerEvent, Separator, Submenu
} from 'react-contexify';
import 'react-contexify/ReactContexify.css';
import { RootState } from "src/stores/store.ts";


const TreeNodeComponent: React.FC<{
  node: TreeNode;
  openEditor: (node: TreeNode) => void;
}> = ({node, openEditor}) => {
  const architectApi = useContext(ArchitectApiContext)!;
  const dispatch = useDispatch();
  const expandedNodes = useSelector(selectExpandedNodes);
  const isExpanded = expandedNodes.includes(node.id);
  const childNodes = useSelector((state: RootState) => SelectChildNodes(state, node.id), shallowEqual)
  const [isLoading, setIsLoading] = useState(false)
  const menuId = 'SideMenu' + node.id;

  useEffect(() => {
    if (isExpanded && node.hasChildNodes && (childNodes.length === 0)) {
      loadChildren();
    }
  }, [isExpanded, childNodes]);


  const {show, hideAll} = useContextMenu({
    id: menuId,
  });

  function handleContextMenu(event: TriggerEvent) {
    show({event, props: {}});
  }

  async function loadChildren() {
    if (isLoading || !node.hasChildNodes) {
      return;
    }
    setIsLoading(true);
    try {
      const nodes = await architectApi.getNodeChildren(node);
      dispatch(setChildNodes({nodeId: node.id, children: nodes}));
    } finally {
      setIsLoading(false);
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
    if (node.hasChildNodes && !isLoading && !isExpanded && (childNodes.length === 0)) { // !isExpanded => will be expanded now
      await loadChildren();
    }
    dispatch(toggleNode(node.id));
  };

  async function handleDelete(){
    await architectApi.deleteSchemaItem(node.schemaItemId);
  }

  function onMenuVisibilityChange(isVisible: boolean) {
    if (isVisible) {
      document.addEventListener('wheel', hideAll);
    }
    else {
      document.removeEventListener('wheel', hideAll);
    }
  }

  return (
    <div className={"treeNode"}>
      <div className={"treeNodeTitle"}>
        <div onClick={onToggle}>
          {node.hasChildNodes ? (isExpanded ? '▼' : '▶') : '•'}
        </div>
        <div
          onDoubleClick={() => onNodeDoubleClick(node)}
          onContextMenu={handleContextMenu}
        >
          {node.nodeText}
        </div>
        <Menu
          id={menuId}
          onVisibilityChange={onMenuVisibilityChange}
        >
          {/*<Submenu label="New" disabled>*/}
            {/*<Item id="reload" onClick={handleItemClick}>Reload</Item>*/}
          {/*</Submenu>*/}
          <Separator/>
          <Item id="edit" onClick={() => onNodeDoubleClick(node)}>Edit</Item>
          <Item id="delete" onClick={handleDelete}>Delete</Item>
        </Menu>
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
  openEditor: (node: TreeNode) => void;
}> = ({openEditor}) => {
  const nodes = useSelector(selectTopNodes);

  return (
    <div>
      {nodes.map((node) => (
        <TreeNodeComponent
          key={node.id + node.nodeText}
          node={node}
          openEditor={openEditor}
          children={node.children} // The top nodes come with preloaded children
        />
      ))}
    </div>
  );
};

export default LazyLoadedTree;