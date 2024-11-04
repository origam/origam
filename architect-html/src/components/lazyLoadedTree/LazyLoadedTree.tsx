import { useContext, useEffect } from 'react';
import "src/components/lazyLoadedTree/LazyLoadedTree.css"
import {
  Menu,
  Item,
  useContextMenu, TriggerEvent, Separator, Submenu
} from 'react-contexify';
import 'react-contexify/ReactContexify.css';
import { TreeNode } from "src/stores/TreeNode.ts";
import { RootStoreContext } from "src/main.tsx";
import { flow } from "mobx";
import { observer } from "mobx-react-lite";

const TreeNodeComponent: React.FC<{
  node: TreeNode;
}> = observer( ({node}) => {
  const projectState = useContext(RootStoreContext).projectState;
  const menuId = 'SideMenu' + node.id;

  useEffect(() => {
    if (node.isExpanded && node.hasChildNodes && (node.children.length === 0)) {
      flow(node.loadChildren.bind(node))();
    }
  }, [node.isExpanded, node.children]);

  const {show, hideAll} = useContextMenu({
    id: menuId,
  });

  function handleContextMenu(event: TriggerEvent) {
    show({event, props: {}});
  }

  const onNodeDoubleClick = async (node: TreeNode) => {
    if (!node.editorType) {
      await onToggle();
    } else {
      projectState.openEditor(node);
    }
  }

  const onToggle = async () => {
    await flow(node.toggle.bind(node))();
  };

  function onMenuVisibilityChange(isVisible: boolean) {
    if (isVisible) {
      document.addEventListener('wheel', hideAll);
    } else {
      document.removeEventListener('wheel', hideAll);
    }
  }

  return (
    <div className={"treeNode"}>
      <div className={"treeNodeTitle"}>
        <div onClick={onToggle}>
          {node.hasChildNodes ? (node.isExpanded ? '▼' : '▶') : '•'}
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
          <Item id="delete" onClick={() => flow(node.delete)()}>Delete</Item>
        </Menu>
        {node.isLoading && ' Loading...'}
      </div>
      {node.isExpanded && node.children.length > 0 && (
        <div>
          {node.children.map((childNode) => (
            <TreeNodeComponent
              key={childNode.id + childNode.nodeText}
              node={childNode}
            />
          ))}
        </div>
      )}
    </div>
  );
});

const LazyLoadedTree: React.FC = observer(() => {
  const projectState = useContext(RootStoreContext).projectState;

  return (
    <div>
      {projectState.modelNodes.map((node) => (
        <TreeNodeComponent
          key={node.id + node.nodeText}
          node={node}
          children={node.children} // The top nodes come with preloaded children
        />
      ))}
    </div>
  );
});

export default LazyLoadedTree;