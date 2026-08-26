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
import {
  CreateFilterType,
  ICreateWizardResult,
  IMoveTargetsResult,
  ISearchResult,
} from '@api/IArchitectApi';
import { Icon } from '@components/icon/Icon';
import S from '@components/modelTree/ModelTree.module.scss';
import { MoveToDialog } from '@components/modelTree/MoveToDialog';
import { TreeNode, toNodeRef } from '@components/modelTree/TreeNode';
import { CreateLookupWizard } from '@components/modelTree/createWizard/CreateLookupWizard';
import { CreateScreenWizard } from '@components/modelTree/createWizard/CreateScreenWizard';
import { CreateWorkQueueWizard } from '@components/modelTree/createWizard/CreateWorkQueueWizard';
import { CreateDataStructureWizard } from '@components/modelTree/createWizard/CreateDataStructureWizard';
import { CreateScreenFromSectionWizard } from '@components/modelTree/createWizard/CreateScreenFromSectionWizard';
import { CreateMenuItemWizard } from '@components/modelTree/createWizard/CreateMenuItemWizard';
import { CreateWorkflowMenuItemWizard } from '@components/modelTree/createWizard/CreateWorkflowMenuItemWizard';
import { CreateRoleWizard } from '@components/modelTree/createWizard/CreateRoleWizard';
import { CreateLocalizationChildEntityWizard } from '@components/modelTree/createWizard/CreateLocalizationChildEntityWizard';
import { CreateScreenSectionWizard } from '@components/modelTree/createWizard/CreateScreenSectionWizard';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { useKeyboardShortcuts } from '@/hooks/useKeyboardShortcuts';
import {
  hasTextSelection,
  isCopyShortcut,
  isCutShortcut,
  isPasteShortcut,
  isTypingTarget,
} from '@/utils/keyShortcuts';
import { observer } from 'mobx-react-lite';
import { DragEvent as ReactDragEvent, useContext, useEffect, useRef } from 'react';
import {
  Item,
  Menu,
  Separator,
  Submenu,
  TriggerEvent,
  useContextMenu,
} from '@origam/react-contexify';
import '@origam/react-contexify/ReactContexify.css';

const DeploymentBadges = observer(({ node }: { node: TreeNode }) => {
  return (
    <>
      {node.deploymentStatus && (
        <span
          className={`${S.statusBadge} ${
            node.deploymentStatus === 'Done' ? S.statusDone : S.statusPending
          }`}
          title={
            node.deploymentStatus === 'Done'
              ? T('Already deployed to the database.', 'tree_node_deployment_status_done_tooltip')
              : T(
                  'Not deployed to the database yet.',
                  'tree_node_deployment_status_pending_tooltip',
                )
          }
        >
          {node.deploymentStatus === 'Done'
            ? T('Done', 'tree_node_deployment_status_done')
            : T('Pending', 'tree_node_deployment_status_pending')}
        </span>
      )}
      {node.isCurrentVersion && (
        <span
          className={S.currentBadge}
          title={T(
            'The version new deployment scripts are added to.',
            'tree_node_deployment_current_tooltip',
          )}
        >
          {T('Current', 'tree_node_deployment_current')}
        </span>
      )}
    </>
  );
});

const AUTO_EXPAND_DELAY_MS = 700;

const ModelTreeNode = observer(({ node, level }: { node: TreeNode; level: number }) => {
  const rootStore = useContext(RootStoreContext);
  const editorTabViewState = rootStore.editorTabViewState;
  const modelTreeState = rootStore.modelTreeState;
  const transfer = modelTreeState.transfer;
  const highlightedNodeId = modelTreeState.highlightedNodeId;
  const highlightToken = modelTreeState.highlightToken;
  const menuId = 'SideMenu' + node.id;
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const nodeRef = useRef<HTMLDivElement | null>(null);
  const autoExpandTimer = useRef<number | null>(null);

  useEffect(() => {
    if (node.isExpanded && !node.childrenInitialized && node.children.length === 0) {
      run({ generator: node.loadChildren.bind(node) });
    }
  }, [node.isExpanded, node.children, node, run]);

  // A pending cut or copy only knows the nodes visible when it started.
  useEffect(() => {
    if (!node.isExpanded || !transfer.hasSource || transfer.hasVerdictsFor(node.children)) {
      return;
    }
    run({ generator: () => transfer.loadTargetVerdicts(node.children) });
  }, [node.isExpanded, node.children, transfer, run]);

  useEffect(
    () => () => {
      if (autoExpandTimer.current !== null) {
        window.clearTimeout(autoExpandTimer.current);
      }
    },
    [],
  );

  const { show, hideAll } = useContextMenu({
    id: menuId,
  });

  async function handleContextMenu(event: TriggerEvent) {
    if (node.nodeLevelType === 'Category') {
      event.preventDefault();
      return;
    }
    onSelect();
    run({ generator: node.getMenuItems.bind(node) });
    show({ event, props: {} });
  }

  function onSelect() {
    modelTreeState.selectNode(node);
  }

  function clearAutoExpand() {
    if (autoExpandTimer.current !== null) {
      window.clearTimeout(autoExpandTimer.current);
      autoExpandTimer.current = null;
    }
  }

  function onCut() {
    onSelect();
    run({ generator: () => transfer.beginTransfer(node, 'cut') });
  }

  function onCopy() {
    onSelect();
    run({ generator: () => transfer.beginTransfer(node, 'copy') });
  }

  function onPaste() {
    run({ generator: () => transfer.drop(node, transfer.clipboardMode === 'copy') });
  }

  function openMoveToDialog() {
    run({
      generator: function* () {
        const result = (yield rootStore.architectApi.getMoveTargets({
          source: toNodeRef(node),
        })) as IMoveTargetsResult;
        const closeDialog = rootStore.dialogStack.pushDialog(
          '',
          <MoveToDialog
            sourceName={node.nodeText}
            targets={result.targets}
            isSourceInActivePackage={result.isSourceInActivePackage}
            isTruncated={result.isTruncated}
            onCancel={() => closeDialog()}
            onConfirm={(target, isCopy) => {
              closeDialog();
              run({
                generator: () =>
                  transfer.moveTo(
                    node,
                    { id: target.id, nodeText: target.nodeText, isNonPersistentItem: false },
                    isCopy,
                  ),
              });
            }}
          />,
          undefined,
          false,
        );
      },
    });
  }

  function onDragStart(event: ReactDragEvent) {
    // An img icon is dragged by the browser itself, even on a row that is not draggable.
    if (!node.canDrag) {
      event.preventDefault();
      return;
    }
    event.dataTransfer.effectAllowed = 'copyMove';
    // Firefox needs attached data to start a drag.
    event.dataTransfer.setData('text/plain', node.nodeText);
    onSelect();
    transfer.beginDrag(event.ctrlKey || event.metaKey);
    run({ generator: () => transfer.beginTransfer(node, 'cut') });
  }

  function onDragOver(event: ReactDragEvent) {
    if (!transfer.isDragging) {
      return;
    }
    event.stopPropagation();
    const isCopy = event.ctrlKey || event.metaKey;
    transfer.setCopyModifier(isCopy);
    transfer.setHoverNode(node.id);
    if (!transfer.mayTransferTo(node, isCopy)) {
      event.dataTransfer.dropEffect = 'none';
      return;
    }
    event.preventDefault();
    event.dataTransfer.dropEffect = isCopy ? 'copy' : 'move';
  }

  function onDragEnter() {
    if (!transfer.isDragging) {
      return;
    }
    clearAutoExpand();
    if (node.isExpanded || !node.canExpand || transfer.isSource(node)) {
      return;
    }
    autoExpandTimer.current = window.setTimeout(() => {
      autoExpandTimer.current = null;
      run({
        generator: function* (): Generator<Promise<any>, void, any> {
          rootStore.uiState.setExpanded(node.id, true);
          if (!node.childrenInitialized) {
            yield* node.loadChildren.bind(node)();
          }
          yield* transfer.loadTargetVerdicts(node.children);
        },
      });
    }, AUTO_EXPAND_DELAY_MS);
  }

  function onDragLeave() {
    clearAutoExpand();
    if (transfer.hoverNodeId === node.id) {
      transfer.setHoverNode(null);
    }
  }

  function onDrop(event: ReactDragEvent) {
    event.preventDefault();
    event.stopPropagation();
    clearAutoExpand();
    const isCopy = event.ctrlKey || event.metaKey;
    run({ generator: () => transfer.dropFromDrag(node, isCopy) });
  }

  function onDragEnd() {
    clearAutoExpand();
    transfer.endDrag();
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
    rootStore.notificationState.pushActionResult({
      title: T('{0} created', 'wizard_created_notification_title', actionLabel),
      results,
      onShowResult: () =>
        rootStore.editorTabViewState.openSearchResults(
          actionLabel,
          results,
          T(
            '{0}: {1}',
            'wizard_created_results_title',
            actionLabel,
            results[0]?.foundIn ?? node.nodeText,
          ),
        ),
    });
  }

  function createFilter(filterType: CreateFilterType, label: string) {
    run({
      generator: function* () {
        const result = (yield rootStore.architectApi.createFilter({
          columnId: node.origamId,
          filterType,
        })) as ICreateWizardResult;
        yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
        showCreatedConfirmation(label, result?.searchResults ?? []);
      },
    });
  }

  function openCreateLookupWizard() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateLookupWizard
        entityId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation(
                T('Lookup', 'wizard_artifact_lookup'),
                result?.searchResults ?? [],
              );
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function openCreateScreenWizard() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateScreenWizard
        entityId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation(
                T('Screen', 'wizard_artifact_screen'),
                result?.searchResults ?? [],
              );
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function openCreateWorkQueueWizard() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateWorkQueueWizard
        entityId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation(
                T('WorkQueue Class', 'wizard_artifact_work_queue_class'),
                result?.searchResults ?? [],
              );
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function openCreateScreenSectionWizard() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateScreenSectionWizard
        entityId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation(
                T('Screen Section', 'wizard_artifact_screen_section'),
                result?.searchResults ?? [],
              );
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function openCreateLocalizationChildEntityWizard() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateLocalizationChildEntityWizard
        entityId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation(
                T('Localization Child Entity', 'wizard_artifact_l10n_child_entity'),
                result?.searchResults ?? [],
              );
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function openCreateDataStructureWizard() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateDataStructureWizard
        entityId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation(
                T('Data Structure', 'wizard_artifact_data_structure'),
                result?.searchResults ?? [],
              );
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function openCreateScreenFromSectionWizard() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateScreenFromSectionWizard
        screenSectionId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation(
                T('Screen', 'wizard_artifact_screen'),
                result?.searchResults ?? [],
              );
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

  function openCreateMenuItemWizard() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateMenuItemWizard
        formId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation(
                T('Menu Item', 'wizard_artifact_menu_item'),
                result?.searchResults ?? [],
              );
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function openCreateWorkflowMenuItemWizard() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateWorkflowMenuItemWizard
        workflowId={node.origamId}
        parentNodeName={node.nodeText}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation(
                T('Menu Item', 'wizard_artifact_menu_item'),
                result?.searchResults ?? [],
              );
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function openCreateRoleWizard() {
    const closeDialog = rootStore.dialogStack.pushDialog(
      '',
      <CreateRoleWizard
        itemId={node.origamId}
        itemName={node.nodeText}
        role={node.role ?? ''}
        onCancel={() => closeDialog()}
        onCreate={result => {
          closeDialog();
          run({
            generator: function* () {
              yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
              showCreatedConfirmation(
                T('Role', 'wizard_artifact_role'),
                result?.searchResults ?? [],
              );
            },
          });
        }}
      />,
      undefined,
      false,
    );
  }

  function getSymbol() {
    if (node.canExpand) {
      return node.isExpanded ? '▼' : '▶';
    }
  }

  const isHighlighted = highlightedNodeId === node.id;
  const isCutSource = transfer.isCutSource(node);
  const canPaste =
    transfer.hasSource &&
    !transfer.isBusy &&
    transfer.mayTransferTo(node, transfer.clipboardMode === 'copy');

  useEffect(() => {
    if (isHighlighted) {
      nodeRef.current?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  }, [isHighlighted, highlightToken]);

  const rowClassNames = [
    isHighlighted ? S.highlighted : '',
    modelTreeState.selectedNodeId === node.id ? S.selected : '',
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
    isCutSource ? S.cutNode : '',
    transfer.isDropHighlighted(node) ? S.dropTarget : '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <>
      <div ref={nodeRef} className={rowClassNames} style={{ paddingLeft: `${level * 20}px` }}>
        <div className={S.treeNodeTitle}>
          <div
            className={S.symbol}
            onClick={onToggle}
            data-test-id={`tree-toggle-${node.nodeText}`}
          >
            {getSymbol()}
          </div>
          <div
            draggable={node.canDrag}
            onDragStart={onDragStart}
            onDragOver={onDragOver}
            onDragEnter={onDragEnter}
            onDragLeave={onDragLeave}
            onDrop={onDrop}
            onDragEnd={onDragEnd}
            onClick={onSelect}
            onDoubleClick={() => onNodeDoubleClick(node)}
            onContextMenu={handleContextMenu}
            className={labelClassNames}
            data-test-id={`tree-node-${node.nodeText}`}
          >
            <div className={S.icon}>
              <Icon src={node.iconUrl ?? '/Icons/generic.svg'} />
            </div>
            {node.nodeText}
            <DeploymentBadges node={node} />
          </div>
          <Menu id={menuId} onVisibilityChange={onMenuVisibilityChange}>
            {node.contextMenuItems.length > 0 ? (
              <Submenu label={T('New', 'tree_node_submenu_new')} data-test-id="tree-menu-new">
                {node.contextMenuItems.map(item => (
                  <Item
                    key={item.typeName + item.caption}
                    id={item.typeName}
                    data-test-id={`tree-menu-new-${item.typeName}`}
                    onClick={() => run({ generator: node.createNode(item.typeName) })}
                  >
                    {item.caption}
                  </Item>
                ))}
              </Submenu>
            ) : (
              <Item id="new" disabled data-test-id="tree-menu-new">
                {T('New', 'tree_node_submenu_new')}
              </Item>
            )}
            {node.isDataEntity && (
              <Submenu label={T('Actions', 'tree_node_submenu_actions')}>
                <Item id="create-lookup" onClick={openCreateLookupWizard}>
                  {T('Create Lookup', 'tree_node_create_lookup')}
                </Item>
                <Item id="create-screen" onClick={openCreateScreenWizard}>
                  {T('Create Screen', 'tree_node_create_screen')}
                </Item>
                <Item id="create-screen-section" onClick={openCreateScreenSectionWizard}>
                  {T('Create Screen Section', 'tree_node_create_screen_section')}
                </Item>
                <Item id="create-workqueue" onClick={openCreateWorkQueueWizard}>
                  {T('Create Workqueue class', 'tree_node_create_workqueue')}
                </Item>
                <Item id="create-data-structure" onClick={openCreateDataStructureWizard}>
                  {T('Create Data Structure', 'tree_node_create_data_structure')}
                </Item>
                <Item
                  id="create-l10n-child-entity"
                  onClick={openCreateLocalizationChildEntityWizard}
                >
                  {T('Create Localization Child Entity', 'tree_node_create_l10n_child_entity')}
                </Item>
              </Submenu>
            )}
            {node.isScreen && (
              <Submenu label={T('Actions', 'tree_node_submenu_actions')}>
                <Item id="create-menu-item" onClick={openCreateMenuItemWizard}>
                  {T('Create Menu Item', 'tree_node_create_menu_item')}
                </Item>
              </Submenu>
            )}
            {node.isScreenSection && (
              <Submenu label={T('Actions', 'tree_node_submenu_actions')}>
                <Item id="create-screen-from-section" onClick={openCreateScreenFromSectionWizard}>
                  {T('Create Screen', 'tree_node_create_screen')}
                </Item>
              </Submenu>
            )}
            {node.isDataStructure && (
              <Submenu label={T('Actions', 'tree_node_submenu_actions')}>
                <Item id="show-sql" onClick={showDataStructureSql}>
                  {T('Show SQL', 'tree_node_show_sql')}
                </Item>
              </Submenu>
            )}
            {node.isSequentialWorkflow && (
              <Submenu label={T('Actions', 'tree_node_submenu_actions')}>
                <Item id="create-workflow-menu-item" onClick={openCreateWorkflowMenuItemWizard}>
                  {T('Create Menu Item', 'tree_node_create_workflow_menu_item')}
                </Item>
              </Submenu>
            )}
            {node.hasSpecificRole && (
              <Submenu label={T('Actions', 'tree_node_submenu_actions')}>
                <Item id="create-role" onClick={openCreateRoleWizard}>
                  {T('Create Role', 'tree_node_create_role')}
                </Item>
              </Submenu>
            )}
            {node.isDataEntityColumn && (
              <Submenu label={T('Actions', 'tree_node_submenu_actions')}>
                <Item
                  id="create-filter-equal"
                  onClick={() => createFilter('Equal', T('Filter (=)', 'filter_label_equal'))}
                >
                  {T('Create (=) Filter', 'tree_node_create_filter_equal')}
                </Item>
                <Item
                  id="create-filter-equal-param"
                  onClick={() =>
                    createFilter(
                      'EqualParam',
                      T('Filter (=) with parameter', 'filter_label_equal_param'),
                    )
                  }
                >
                  {T('Create (=) Filter With Parameter', 'tree_node_create_filter_equal_param')}
                </Item>
                <Item
                  id="create-filter-like"
                  onClick={() => createFilter('Like', T('Filter (Like)', 'filter_label_like'))}
                >
                  {T('Create (Like) Filter', 'tree_node_create_filter_like')}
                </Item>
                <Item
                  id="create-filter-like-param"
                  onClick={() =>
                    createFilter(
                      'LikeParam',
                      T('Filter (Like) with parameter', 'filter_label_like_param'),
                    )
                  }
                >
                  {T('Create (Like) Filter With Parameter', 'tree_node_create_filter_like_param')}
                </Item>
                <Item
                  id="create-filter-list-param"
                  onClick={() =>
                    createFilter(
                      'InList',
                      T('Filter (List) with parameter', 'filter_label_list_param'),
                    )
                  }
                >
                  {T('Create (List) Filter With Parameter', 'tree_node_create_filter_list_param')}
                </Item>
                <Item
                  id="create-filter-between"
                  onClick={() =>
                    createFilter(
                      'Between',
                      T('Filter (Between) with parameters', 'filter_label_between_param'),
                    )
                  }
                >
                  {T(
                    'Create (Between) Filter With Parameters',
                    'tree_node_create_filter_between_param',
                  )}
                </Item>
              </Submenu>
            )}
            <Separator />
            <Item
              id="cut"
              data-test-id="tree-menu-cut"
              disabled={!node.canDrag || !node.isInActivePackage}
              onClick={onCut}
            >
              {T('Cut', 'tree_node_cut')}
            </Item>
            <Item id="copy" data-test-id="tree-menu-copy" disabled={!node.canDrag} onClick={onCopy}>
              {T('Copy', 'tree_node_copy')}
            </Item>
            <Item id="paste" data-test-id="tree-menu-paste" disabled={!canPaste} onClick={onPaste}>
              {T('Paste', 'tree_node_paste')}
            </Item>
            <Item
              id="move-to"
              data-test-id="tree-menu-move-to"
              disabled={!node.canDrag}
              onClick={openMoveToDialog}
            >
              {T('Move to...', 'tree_node_move_to')}
            </Item>
            <Separator />
            {!node.isNonPersistentItem && (
              <Item id="edit" data-test-id="tree-menu-edit" onClick={() => onNodeDoubleClick(node)}>
                {T('Edit', 'tree_node_edit')}
              </Item>
            )}
            {!node.isNonPersistentItem && (
              <Item id="delete" data-test-id="tree-menu-delete" onClick={onDelete}>
                {T('Delete', 'tree_node_delete')}
              </Item>
            )}
            {!node.isNonPersistentItem && (
              <Item
                id="documentation"
                data-test-id="tree-menu-documentation"
                onClick={openDocumentationEditor}
              >
                {T('Documentation', 'tree_node_documentation')}
              </Item>
            )}
            {!node.isNonPersistentItem && (
              <Item id="references" data-test-id="tree-menu-references" onClick={findReferences}>
                {T('Find references', 'tree_node_references')}
              </Item>
            )}
            {!node.isNonPersistentItem && (
              <Item
                id="dependencies"
                data-test-id="tree-menu-dependencies"
                onClick={findDependencies}
              >
                {T('Find dependencies', 'tree_node_dependencies')}
              </Item>
            )}
            {node.isDeploymentVersion && <Separator />}
            {node.isDeploymentVersion && (
              <Item
                id="setVersionCurrent"
                data-test-id="tree-menu-set-version-current"
                onClick={setVersionCurrent}
              >
                {T('Make version current', 'tree_node_make_version_current')}
              </Item>
            )}
            {node.isUpdateScriptActivity && <Separator />}
            {node.isUpdateScriptActivity && (
              <Item
                id="runUpdateScriptActivity"
                data-test-id="tree-menu-run-update-script-activity"
                onClick={runUpdateScriptActivity}
              >
                {T('Execute', 'tree_node_run_update_script_activity')}
              </Item>
            )}
          </Menu>
          {node.isLoading && (
            <span className={S.loading}>{T('Loading...', 'tree_node_loading')}</span>
          )}
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
  const rootStore = useContext(RootStoreContext);
  const modelTreeState = rootStore.modelTreeState;
  const transfer = modelTreeState.transfer;
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const treeRef = useRef<HTMLDivElement | null>(null);

  // dragover fires on movement only, the highlight has to follow Ctrl too.
  useEffect(() => {
    if (!transfer.isDragging) {
      return;
    }
    const onModifierChange = (e: KeyboardEvent) => {
      transfer.setCopyModifier(e.ctrlKey || e.metaKey);
    };
    window.addEventListener('keydown', onModifierChange);
    window.addEventListener('keyup', onModifierChange);
    return () => {
      window.removeEventListener('keydown', onModifierChange);
      window.removeEventListener('keyup', onModifierChange);
    };
  }, [transfer.isDragging, transfer]);

  const hasTreeFocus = () => !!treeRef.current && treeRef.current.contains(document.activeElement);

  const canPasteInto = (node: TreeNode | null) =>
    transfer.hasSource &&
    !transfer.isBusy &&
    !!node &&
    transfer.mayTransferTo(node, transfer.clipboardMode === 'copy');

  useKeyboardShortcuts([
    {
      predicate: e =>
        isCutShortcut(e) &&
        !isTypingTarget(e) &&
        !hasTextSelection() &&
        hasTreeFocus() &&
        !!modelTreeState.selectedNode?.canDrag &&
        !!modelTreeState.selectedNode?.isInActivePackage,
      handler: () => {
        const node = modelTreeState.selectedNode!;
        run({ generator: () => transfer.beginTransfer(node, 'cut') });
      },
    },
    {
      predicate: e =>
        isCopyShortcut(e) &&
        !isTypingTarget(e) &&
        !hasTextSelection() &&
        hasTreeFocus() &&
        !!modelTreeState.selectedNode?.canDrag,
      handler: () => {
        const node = modelTreeState.selectedNode!;
        run({ generator: () => transfer.beginTransfer(node, 'copy') });
      },
    },
    {
      predicate: e =>
        isPasteShortcut(e) &&
        !isTypingTarget(e) &&
        hasTreeFocus() &&
        canPasteInto(modelTreeState.selectedNode),
      handler: () => {
        const node = modelTreeState.selectedNode!;
        run({ generator: () => transfer.drop(node, transfer.clipboardMode === 'copy') });
      },
    },
    {
      predicate: e => e.key === 'Escape' && hasTreeFocus() && transfer.hasSource,
      handler: () => transfer.clear(),
    },
  ]);

  return (
    <div
      ref={treeRef}
      className={S.root}
      tabIndex={0}
      onMouseDown={() => treeRef.current?.focus({ preventScroll: true })}
      // An outside file dropped on empty space must not navigate away.
      onDragOver={e => {
        if (transfer.isDragging) {
          e.dataTransfer.dropEffect = 'none';
          return;
        }
        e.preventDefault();
      }}
      onDrop={e => e.preventDefault()}
    >
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
