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
import { CreateFilterType, ICreateActionResult, ISearchResult } from '@api/IArchitectApi';
import { Icon } from '@components/icon/Icon';
import S from '@components/modelTree/ModelTree.module.scss';
import { TreeNode } from '@components/modelTree/TreeNode';
import { CreateLookupDrawer } from '@components/modelTree/createWizard/CreateLookupDrawer';
import { CreateScreenDrawer } from '@components/modelTree/createWizard/CreateScreenDrawer';
import { CreateWorkQueueDrawer } from '@components/modelTree/createWizard/CreateWorkQueueDrawer';
import { CreateMenuItemDrawer } from '@components/modelTree/createWizard/CreateMenuItemDrawer';
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
    if (node.nodeLevelType === 'Category') {
      event.preventDefault();
      return;
    }
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

  function showCreatedConfirmation(actionLabel: string, results: ISearchResult[]) {
    rootStore.toastState.pushActionResult({
      title: `${actionLabel} created`,
      results,
      onShowResult: () =>
        rootStore.editorTabViewState.openSearchResults(
          actionLabel,
          results,
          `${actionLabel}: ${results[0]?.foundIn ?? node.nodeText}`,
        ),
    });
  }

  function createFilter(filterType: CreateFilterType, label: string) {
    run({
      generator: function* () {
        const result = (yield rootStore.architectApi.createFilter({
          columnId: node.origamId,
          filterType,
        })) as ICreateActionResult;
        yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
        showCreatedConfirmation(label, result?.searchResults ?? []);
      },
    });
  }

  function openCreateLookupDrawer() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateLookupDrawer
        entityId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation('Lookup', result?.searchResults ?? []);
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function openCreateScreenDrawer() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateScreenDrawer
        entityId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation('Screen', result?.searchResults ?? []);
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function openCreateWorkQueueDrawer() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateWorkQueueDrawer
        entityId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation('WorkQueue Class', result?.searchResults ?? []);
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function showDataStructureSql() {
    run({
      generator: function* () {
        const result = yield rootStore.architectApi.getDataStructureSql(node.origamId);
        rootStore.editorTabViewState.openShowSqlEditor(
          result.dataStructureId,
          result.dataStructureName,
          result.sql,
        );
      },
    });
  }

  function openCreateMenuItemDrawer() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateMenuItemDrawer
        formId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation('Menu Item', result?.searchResults ?? []);
            },
          });
        }}
      />,
      undefined,
      false,
    );
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

  const rowClassNames = [
    isHighlighted ? S.highlighted : '',
    node.nodeLevelType === 'Category' ? S.categoryNode : '',
    node.nodeLevelType === 'Provider' ? S.providerNode : '',
  ]
    .filter(Boolean)
    .join(' ');

  const labelClassNames = [
    S.iconAndText,
    node.isCurrentVersion ? S.currentVersion : '',
    !node.isInActivePackage && !node.isFileDirty ? S.crossPackage : '',
    node.isFileDirty ? S.dirty : '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <>
      <div ref={nodeRef} className={rowClassNames} style={{ paddingLeft: `${level * 20}px` }}>
        <div className={S.treeNodeTitle}>
          <div className={S.symbol} onClick={onToggle}>
            {getSymbol()}
          </div>
          <div
            onDoubleClick={() => onNodeDoubleClick(node)}
            onContextMenu={handleContextMenu}
            className={labelClassNames}
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
            {node.isDataEntity && (
              <Submenu label="Actions">
                <Item id="create-lookup" onClick={openCreateLookupDrawer}>
                  Create Lookup
                </Item>
                <Item id="create-screen" onClick={openCreateScreenDrawer}>
                  Create Screen
                </Item>
                <Item id="create-workqueue" onClick={openCreateWorkQueueDrawer}>
                  Create Workqueue class
                </Item>
              </Submenu>
            )}
            {node.isScreen && (
              <Submenu label="Actions">
                <Item id="create-menu-item" onClick={openCreateMenuItemDrawer}>
                  Create Menu Item
                </Item>
              </Submenu>
            )}
            {node.isDataStructure && (
              <Submenu label="Actions">
                <Item id="show-sql" onClick={showDataStructureSql}>
                  Show SQL
                </Item>
              </Submenu>
            )}
            {node.isDataEntityColumn && (
              <Submenu label="Actions">
                <Item id="create-filter-equal" onClick={() => createFilter('Equal', 'Filter (=)')}>
                  Create (=) Filter
                </Item>
                <Item
                  id="create-filter-equal-param"
                  onClick={() => createFilter('EqualParam', 'Filter (=) with parameter')}
                >
                  Create (=) Filter With Parameter
                </Item>
                <Item id="create-filter-like" onClick={() => createFilter('Like', 'Filter (Like)')}>
                  Create (Like) Filter
                </Item>
                <Item
                  id="create-filter-like-param"
                  onClick={() => createFilter('LikeParam', 'Filter (Like) with parameter')}
                >
                  Create (Like) Filter With Parameter
                </Item>
                <Item
                  id="create-filter-list-param"
                  onClick={() => createFilter('InList', 'Filter (List) with parameter')}
                >
                  Create (List) Filter With Parameter
                </Item>
                <Item
                  id="create-filter-between"
                  onClick={() => createFilter('Between', 'Filter (Between) with parameters')}
                >
                  Create (Between) Filter With Parameters
                </Item>
              </Submenu>
            )}
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
          {node.isLoading && <span className={S.loading}>Loading...</span>}
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
    <div className={S.root}>
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
