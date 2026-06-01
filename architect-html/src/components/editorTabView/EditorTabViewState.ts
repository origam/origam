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

import {
  EditorType,
  IApiEditorNode,
  IApiTabData,
  IArchitectApi,
  IDatabaseResultResponse,
  ISearchResult,
} from '@api/IArchitectApi';
import { EditorData } from '@components/modelTree/EditorData';
import { TreeNode } from '@components/modelTree/TreeNode';
import { askYesNoQuestion, YesNoResult } from '@dialogs/DialogUtils';
import { EditorContainer } from '@editors/EditorContainer.tsx';
import { getEditorContainer } from '@editors/getEditorContainer.tsx';
import { SearchResultsTabState } from '@components/search/SearchResultsTabState.ts';
import { FlowHandlerInput, runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { RootStore } from '@stores/RootStore';
import { observable } from 'mobx';
import { CancellablePromise } from 'mobx/dist/api/flow';

const SearchEditorId = 'SearchResultsEditor-Id';
const DeploymentScriptsGeneratorModuleId = 'DeploymentScriptsGeneratorModule-Id';

export class EditorTabViewState {
  @observable accessor editorsContainers: EditorContainer[] = [];
  architectApi: IArchitectApi;
  runGeneratorHandled: (args: FlowHandlerInput) => CancellablePromise<any>;

  constructor(private rootStore: RootStore) {
    this.architectApi = this.rootStore.architectApi;
    this.runGeneratorHandled = runInFlowWithHandler(rootStore.errorDialogController);
  }

  *initializeOpenEditors(): Generator<Promise<unknown>, void, any> {
    const openTabsData = (yield this.architectApi.getOpenTabs()) as IApiTabData[];
    this.editorsContainers = openTabsData.map(data => this.toEditor(data)) as EditorContainer[];
    if (this.editorsContainers.length > 0) {
      this.setActiveEditor(this.editorsContainers[this.editorsContainers.length - 1].state.tabId);
    }

    if (this.rootStore.uiState.getDsGeneratorState().isOpen) {
      try {
        yield* this.openDeploymentScriptsGeneratorModule()();
      } catch (err) {
        console.error('Failed to auto-open Deployment Scripts Generator module:', err);
      }
    }
  }

  private toEditor(data: IApiTabData) {
    const treeNode = this.rootStore.modelTreeState.findNodeById(data.node.id);
    const editorData = new EditorData(data, treeNode);

    return getEditorContainer({
      editorType: editorData.editorType,
      editorData: editorData,
      propertiesState: this.rootStore.propertiesState,
      architectApi: this.architectApi,
      modelTreeState: this.rootStore.modelTreeState,
      uiState: this.rootStore.uiState,
      runGeneratorHandled: this.runGeneratorHandled,
    });
  }

  openEditorById(node: TreeNode) {
    return function* (
      this: EditorTabViewState,
    ): Generator<Promise<IApiTabData>, void, IApiTabData> {
      const apiTabData = yield this.architectApi.openTab(node.origamId);
      const editorData = new EditorData(apiTabData, node);
      this.openEditor(editorData);
    }.bind(this);
  }

  openDocumentationEditor(node: TreeNode) {
    return function* (
      this: EditorTabViewState,
    ): Generator<Promise<IApiTabData>, void, IApiTabData> {
      const apiTabData = yield this.architectApi.openDocumentationEditor(node.origamId);
      const editorData = new EditorData(apiTabData, node);
      this.openEditor(editorData, 'DocumentationEditor');
    }.bind(this);
  }

  openDeploymentScriptsGeneratorModule() {
    return function* (
      this: EditorTabViewState,
    ): Generator<Promise<IDatabaseResultResponse>, void, IDatabaseResultResponse> {
      const response = yield this.architectApi.fetchDeploymentScriptsList(null);

      const tempTabData: IApiTabData = {
        tabId: DeploymentScriptsGeneratorModuleId,
        tabType: 'DeploymentScriptsGeneratorModule' as EditorType,
        parentNodeId: undefined,
        isDirty: false,
        node: {
          id: '',
          origamId: '',
          nodeText: '',
          editorType: 'DeploymentScriptsGeneratorModule',
        },
        data: {
          possibleDeploymentVersions: response.deploymentVersions,
          currentDeploymentVersionId: response.currentDeploymentVersionId,
          results: response.results,
        },
      };

      const editorData = new EditorData(tempTabData, null);
      this.openEditor(editorData, 'DeploymentScriptsGeneratorModule');
      this.rootStore.uiState.setDsGeneratorState({ isOpen: true });
    }.bind(this);
  }

  openSearchResults(queryText: string, results: ISearchResult[], label: string) {
    const existingEditor = this.editorsContainers.find(
      editor => editor.state instanceof SearchResultsTabState,
    );
    if (existingEditor) {
      const editorState = existingEditor.state as SearchResultsTabState;
      editorState.query = queryText;
      editorState.results = results;
      editorState.label = label;
      this.setActiveEditor(editorState.tabId);
      return;
    }

    const tempTabData: IApiTabData = {
      tabId: SearchEditorId,
      tabType: 'SearchResultsEditor',
      parentNodeId: undefined,
      isDirty: false,
      node: {
        id: '',
        origamId: '',
        nodeText: '',
        editorType: null,
      },
      data: {
        query: queryText,
        results,
      },
    };

    const editorData = new EditorData(tempTabData, null);
    this.openEditor(editorData);
  }

  openEditor(editorData: EditorData, editorType?: EditorType) {
    const alreadyOpenEditor = this.editorsContainers.find(
      editor => editor.state.tabId === editorData.editorId,
    );
    if (alreadyOpenEditor) {
      this.setActiveEditor(alreadyOpenEditor.state.tabId);
      return;
    }

    const editor = getEditorContainer({
      editorType: editorType === undefined ? editorData.editorType : editorType,
      editorData: editorData,
      propertiesState: this.rootStore.propertiesState,
      architectApi: this.architectApi,
      modelTreeState: this.rootStore.modelTreeState,
      uiState: this.rootStore.uiState,
      runGeneratorHandled: this.runGeneratorHandled,
    });
    if (!editor) {
      return;
    }

    this.editorsContainers.push(editor);
    this.setActiveEditor(editor.state.tabId);
  }

  get activeEditorState() {
    return this.editorsContainers.find(editor => editor.state.isActive)?.state;
  }

  setActiveEditor(schemaItemId: string) {
    for (const editor of this.editorsContainers) {
      editor.state.isActive = editor.state.tabId === schemaItemId;
    }
  }

  closeAllEditors() {
    return function* (this: EditorTabViewState): Generator<Promise<any>, boolean, any> {
      const dirtyEditors = this.editorsContainers.filter(editor => editor.state.isDirty);
      if (dirtyEditors.length > 0) {
        const saveChanges = yield askYesNoQuestion(
          this.rootStore.dialogStack,
          'Save changes',
          dirtyEditors.length === 1
            ? `Do you want to save ${dirtyEditors[0].state.label}?`
            : `Do you want to save changes to ${dirtyEditors.length} open tabs?`,
        );
        switch (saveChanges) {
          case YesNoResult.Cancel:
            return false;
          case YesNoResult.Yes:
            for (const editor of dirtyEditors) {
              yield* editor.state.save();
            }
            break;
          case YesNoResult.No:
            break;
        }
      }

      for (const editor of this.editorsContainers) {
        editor.state.dispose?.();
      }
      this.editorsContainers = [];
      yield this.architectApi.closeAllTabs();
      this.rootStore.uiState.setDsGeneratorState({ isOpen: false });
      return true;
    }.bind(this);
  }

  closeEditor(editorId: string) {
    return function* (this: EditorTabViewState): Generator<Promise<any>, void, any> {
      if (this.activeEditorState?.isDirty) {
        const saveChanges = yield askYesNoQuestion(
          this.rootStore.dialogStack,
          'Save changes',
          `Do you want to save ${this.activeEditorState.label}?`,
        );
        switch (saveChanges) {
          case YesNoResult.No:
            break;
          case YesNoResult.Cancel:
            return;
          case YesNoResult.Yes:
            yield* this.activeEditorState.save();
            break;
        }
      }

      const closingEditor = this.editorsContainers.find(
        (editor: EditorContainer) => editor.state.tabId === editorId,
      );
      closingEditor?.state.dispose?.();

      this.editorsContainers = this.editorsContainers.filter(
        (editor: EditorContainer) => editor.state.tabId !== editorId,
      );

      if (editorId === DeploymentScriptsGeneratorModuleId) {
        this.rootStore.uiState.setDsGeneratorState({ isOpen: false });
      } else if (editorId !== SearchEditorId) {
        yield this.architectApi.closeTab(editorId);
      }

      if (this.editorsContainers.length > 0) {
        const editorToActivate = this.editorsContainers[this.editorsContainers.length - 1];
        this.setActiveEditor(editorToActivate.state.tabId);
      }
    }.bind(this);
  }
}

export interface IEditorNode extends IApiEditorNode {
  parent: TreeNode | null;
}
