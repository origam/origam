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

import { RootStoreContext, T } from '@/main';
import { ISearchResult } from '@api/IArchitectApi';
import { Icon } from '@components/icon/Icon';
import S from '@components/modelTree/ModelTree.module.scss';
import { TreeNode } from '@components/modelTree/TreeNode';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import { useContext, useEffect, useRef } from 'react';
import { Item, Menu, Separator, Submenu, TriggerEvent, useContextMenu } from 'react-contexify';
import 'react-contexify/ReactContexify.css';

const ModelTreeNode = observer(({ node, level }: { node: TreeNode; level: number }) => {
  const rootStore = useContext(RootStoreContext);
  const editorTabViewState = rootStore.editorTabViewState;
  const highlightedNodeId = rootStore.modelTreeState.highlightedNodeId;
  const highlightToken = rootStore.modelTreeState.highlightToken;
  const menuId = 'SideMenu' + node.id;
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const nodeRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (node.isExpanded && !node.childrenInitialized && node.children.length === 0) {
      run({ generator: node.loadChildren.bind(node) });
    }
  }, [node.isExpanded, node.children, node, run]);

  const { show, hideAll } = useContextMenu({
    id: menuId,
  });

  async function handleContextMenu(event: TriggerEvent) {
    run({ generator: node.getMenuItems.bind(node) });
    show({ event, props: {} });
  }

  const onNodeDoubleClick = async (node: TreeNode) => {
    if (!node.editorType) {
      await onToggle();
    } else {
      run({ generator: editorTabViewState.openEditorById(node) });
    }
  };

  const onToggle = async () => {
    run({ generator: node.toggle.bind(node) });
  };

  function onMenuVisibilityChange(isVisible: boolean) {
    if (isVisible) {
      document.addEventListener('wheel', hideAll);
    } else {
      document.removeEventListener('wheel', hideAll);
    }
  }

  function onDelete() {
    run({ generator: node.delete.bind(node) });
  }

  function openDocumentationEditor() {
    run({ generator: editorTabViewState.openDocumentationEditor(node) });
  }

  function findReferences() {
    run({
      generator: function* () {
        const results = (yield rootStore.architectApi.searchReferences(
          node.origamId,
        )) as ISearchResult[];
        rootStore.editorTabViewState.openSearchResults(
          node.nodeText,
          results,
          T('References of: {0}', 'editor_search_results_references_title', node.nodeText),
        );
      },
    });
  }

  function findDependencies() {
    run({
      generator: function* () {
        const results = (yield rootStore.architectApi.searchDependencies(
          node.origamId,
        )) as ISearchResult[];
        rootStore.editorTabViewState.openSearchResults(
          node.nodeText,
          results,
          T('Dependencies of: {0}', 'editor_search_results_dependencies_title', node.nodeText),
        );
      },
    });
  }

  function setVersionCurrent() {
    run({ generator: node.setVersionCurrent() });
  }

  function runUpdateScriptActivity() {
    run({ generator: node.runUpdateScriptActivity() });
  }

  function getSymbol() {
    if (node.children.length > 0 || !node.childrenInitialized) {
      return node.isExpanded ? '▼' : '▶';
    }
  }

  const isHighlighted = highlightedNodeId === node.id;

  useEffect(() => {
    if (isHighlighted) {
      nodeRef.current?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  }, [isHighlighted, highlightToken]);

  return (
    <>
      <div
        ref={nodeRef}
        className={isHighlighted ? S.highlighted : ''}
        style={{ paddingLeft: `${level * 20}px` }}
      >
        <div className={S.treeNodeTitle}>
          <div className={S.symbol} onClick={onToggle}>
            {getSymbol()}
          </div>
          <div
            onDoubleClick={() => onNodeDoubleClick(node)}
            onContextMenu={handleContextMenu}
            className={`${S.iconAndText} ${node.isCurrentVersion ? S.currentVersion : ''}`}
          >
            <div className={S.icon}>
              <Icon src={node.iconUrl ?? '/Icons/generic.svg'} />
            </div>
            {node.nodeText}
          </div>
          <Menu id={menuId} onVisibilityChange={onMenuVisibilityChange}>
            <Submenu label="New">
              {node.contextMenuItems.map(item => (
                <Item
                  key={item.typeName + item.caption}
                  id={item.typeName}
                  onClick={() => run({ generator: node.createNode(item.typeName) })}
                >
                  {item.caption}
                </Item>
              ))}
            </Submenu>
            <Separator />
            {!node.isNonPersistentItem && (
              <Item id="edit" onClick={() => onNodeDoubleClick(node)}>
                {T('Edit', 'tree_node_edit')}
              </Item>
            )}
            {!node.isNonPersistentItem && (
              <Item id="delete" onClick={onDelete}>
                {T('Delete', 'tree_node_delete')}
              </Item>
            )}
            {!node.isNonPersistentItem && (
              <Item id="documentation" onClick={openDocumentationEditor}>
                {T('Documentation', 'tree_node_documentation')}
              </Item>
            )}
            {!node.isNonPersistentItem && (
              <Item id="references" onClick={findReferences}>
                {T('Find references', 'tree_node_references')}
              </Item>
            )}
            {!node.isNonPersistentItem && (
              <Item id="dependencies" onClick={findDependencies}>
                {T('Find dependencies', 'tree_node_dependencies')}
              </Item>
            )}
            {node.isDeploymentVersion && <Separator />}
            {node.isDeploymentVersion && (
              <Item id="setVersionCurrent" onClick={setVersionCurrent}>
                {T('Make version current', 'tree_node_make_version_current')}
              </Item>
            )}
            {node.isUpdateScriptActivity && <Separator />}
            {node.isUpdateScriptActivity && (
              <Item id="runUpdateScriptActivity" onClick={runUpdateScriptActivity}>
                {T('Execute', 'tree_node_run_update_script_activity')}
              </Item>
            )}
          </Menu>
          {node.isLoading && ' Loading...'}
        </div>
      </div>
      {node.isExpanded &&
        node.children.length > 0 &&
        node.children.map(childNode => (
          <ModelTreeNode
            key={childNode.id + childNode.nodeText}
            node={childNode}
            level={level + 1}
          />
        ))}
    </>
  );
});

const ModelTree = observer(() => {
  const modelTreeState = useContext(RootStoreContext).modelTreeState;

  return (
    <div>
      {modelTreeState.activePackageName && (
        <div className={S.packageName}>{modelTreeState.activePackageName}</div>
      )}
      {modelTreeState.modelNodes.map(node => (
        <ModelTreeNode key={node.id + node.nodeText} node={node} level={0} />
      ))}
    </div>
  );
});

export default ModelTree;
