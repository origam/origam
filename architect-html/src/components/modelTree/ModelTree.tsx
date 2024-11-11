import { useContext, useEffect } from 'react';
import S from "src/components/modelTree/ModelTree.module.scss"
import {
  Menu,
  Item,
  useContextMenu, TriggerEvent, Separator, Submenu
} from 'react-contexify';
import 'react-contexify/ReactContexify.css';
import { TreeNode } from "src/components/modelTree/TreeNode.ts";
import { RootStoreContext } from "src/main.tsx";
import { observer } from "mobx-react-lite";
import {
  runGeneratorInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";

const ModelTreeNode: React.FC<{
  node: TreeNode;
}> = observer(({node}) => {
  const rootStore = useContext(RootStoreContext);
  const editorTabViewState = rootStore.editorTabViewState;
  const menuId = 'SideMenu' + node.id;

  useEffect(() => {
    if (node.isExpanded && node.hasChildNodes && (node.children.length === 0)) {
      runGeneratorInFlowWithHandler({
        controller: rootStore.errorDialogController,
        generator: node.loadChildren.bind(node),
      });
    }
  }, [node.isExpanded, node.children]);

  const {show, hideAll} = useContextMenu({
    id: menuId,
  });

  async function handleContextMenu(event: TriggerEvent) {
    await node.getMenuItems();

    // if (node.isNonPersistentItem) {
    //   return;
    // }
    show({event, props: {}});
  }

  const onNodeDoubleClick = async (node: TreeNode) => {
    if (!node.editorType) {
      await onToggle();
    } else {
      editorTabViewState.openEditor(node);
    }
  }

  const onToggle = async () => {
    runGeneratorInFlowWithHandler({
      controller: rootStore.errorDialogController,
      generator: node.toggle.bind(node),
    });
  };

  function onMenuVisibilityChange(isVisible: boolean) {
    if (isVisible) {
      document.addEventListener('wheel', hideAll);
    } else {
      document.removeEventListener('wheel', hideAll);
    }
  }

  function onDelete() {
    runGeneratorInFlowWithHandler({
      controller: rootStore.errorDialogController,
      generator: node.delete.bind(node),
    });
  }

  return (
    <div className={S.treeNode}>
      <div className={S.treeNodeTitle}>
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
          <Submenu label="New">
            {node.contextMenuItems.map((item) => (
              <Item
                key={item.typeName + item.caption}
                id={item.typeName}
                onClick={() => node.createNew(item.typeName)}
              >
                {item.caption}
              </Item>
            ))}
          </Submenu>
          <Separator/>
          {node.isNonPersistentItem &&
            <>
              <Item
                id="edit"
                onClick={() => onNodeDoubleClick(node)}
              >
                Edit
              </Item>
              <Item
                id="delete"
                onClick={onDelete}
              >
                Delete
              </Item>
            </>
          }
        </Menu>
        {node.isLoading && ' Loading...'}
      </div>
      {node.isExpanded && node.children.length > 0 && (
        <div>
          {node.children.map((childNode) => (
            <ModelTreeNode
              key={childNode.id + childNode.nodeText}
              node={childNode}
            />
          ))}
        </div>
      )}
    </div>
  );
});

const ModelTree: React.FC = observer(() => {
  const modelTreeState = useContext(RootStoreContext).modelTreeState;

  return (
    <div>
      {modelTreeState.modelNodes.map((node) => (
        <ModelTreeNode
          key={node.id + node.nodeText}
          node={node}
        />
      ))}
    </div>
  );
});

export default ModelTree;