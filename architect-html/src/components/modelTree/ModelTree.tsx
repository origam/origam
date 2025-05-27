/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import React, { useContext, useEffect } from 'react';
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
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import { Icon } from "src/components/icon/Icon.tsx";

const ModelTreeNode: React.FC<{
  node: TreeNode;
}> = observer(({node}) => {
  const rootStore = useContext(RootStoreContext);
  const editorTabViewState = rootStore.editorTabViewState;
  const menuId = 'SideMenu' + node.id;
  const run = runInFlowWithHandler(rootStore.errorDialogController);

  useEffect(() => {
    if (node.isExpanded && !node.childrenInitialized && (node.children.length === 0)) {
      run({generator: node.loadChildren.bind(node)});
    }
  }, [node.isExpanded, node.children]);

  const {show, hideAll} = useContextMenu({
    id: menuId,
  });

  async function handleContextMenu(event: TriggerEvent) {
    run({generator: node.getMenuItems.bind(node)});
    show({event, props: {}});
  }

  const onNodeDoubleClick = async (node: TreeNode) => {
    if (!node.editorType) {
      await onToggle();
    } else {
      run({generator: editorTabViewState.openEditorById(node)});
    }
  }

  const onToggle = async () => {
    run({generator: node.toggle.bind(node)});
  };

  function onMenuVisibilityChange(isVisible: boolean) {
    if (isVisible) {
      document.addEventListener('wheel', hideAll);
    } else {
      document.removeEventListener('wheel', hideAll);
    }
  }

  function onDelete() {
    run({generator: node.delete.bind(node)});
  }

  function getSymbol() {
    if (node.children.length > 0 || !node.childrenInitialized) {
      return node.isExpanded ? '▼' : '▶'
    }
  }

  return (
    <div className={S.treeNode}>
      <div className={S.treeNodeTitle}>
        <div className={S.symbol} onClick={onToggle}>
          {getSymbol()}
        </div>
        <div
          onDoubleClick={() => onNodeDoubleClick(node)}
          onContextMenu={handleContextMenu}
          className={S.iconAndText}
        >
          <div className={S.icon}>
            <Icon src={node.iconUrl ?? '/Icons/generic.svg'}/>
          </div>
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
                onClick={() => run({generator: node.createNode(item.typeName)})}
              >
                {item.caption}
              </Item>
            ))}
          </Submenu>
          <Separator/>
          {!node.isNonPersistentItem &&
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
        <div className={S.children}>
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