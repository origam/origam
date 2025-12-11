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
  IApiEditorData,
  IApiEditorNode,
  IArchitectApi,
  IDatabaseResultResponse,
} from '@api/IArchitectApi';
import { EditorData } from '@components/modelTree/EditorData';
import { TreeNode } from '@components/modelTree/TreeNode';
import { askYesNoQuestion, YesNoResult } from '@dialogs/DialogUtils';
import { EditorContainer } from '@editors/EditorContainer.tsx';
import { getEditorContainer } from '@editors/getEditorContainer.tsx';
import { FlowHandlerInput, runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { RootStore } from '@stores/RootStore';
import { observable } from 'mobx';
import { CancellablePromise } from 'mobx/dist/api/flow';

export class EditorTabViewState {
  @observable accessor editorsContainers: EditorContainer[] = [];
  architectApi: IArchitectApi;
  runGeneratorHandled: (args: FlowHandlerInput) => CancellablePromise<any>;

  constructor(private rootStore: RootStore) {
    this.architectApi = this.rootStore.architectApi;
    this.runGeneratorHandled = runInFlowWithHandler(rootStore.errorDialogController);
  }

  *initializeOpenEditors(): Generator<Promise<IApiEditorData[]>, void, IApiEditorData[]> {
    const openEditorsData = (yield this.architectApi.getOpenEditors()) as IApiEditorData[];
    this.editorsContainers = openEditorsData.map(data => this.toEditor(data)) as EditorContainer[];
    if (this.editorsContainers.length > 0) {
      this.setActiveEditor(
        this.editorsContainers[this.editorsContainers.length - 1].state.editorId,
      );
    }
  }

  private toEditor(data: IApiEditorData) {
    const treeNode = this.rootStore.modelTreeState.findNodeById(data.node.id);
    const editorData = new EditorData(data, treeNode);

    return getEditorContainer({
      editorType: editorData.editorType,
      editorData: editorData,
      propertiesState: this.rootStore.propertiesState,
      architectApi: this.architectApi,
      modelTreeState: this.rootStore.modelTreeState,
      runGeneratorHandled: this.runGeneratorHandled,
    });
  }

  openEditorById(node: TreeNode) {
    return function* (
      this: EditorTabViewState,
    ): Generator<Promise<IApiEditorData>, void, IApiEditorData> {
      const apiEditorData = yield this.architectApi.openEditor(node.origamId);
      const editorData = new EditorData(apiEditorData, node);
      this.openEditor(editorData);
    }.bind(this);
  }

  openDocumentationEditor(node: TreeNode) {
    return function* (
      this: EditorTabViewState,
    ): Generator<Promise<IApiEditorData>, void, IApiEditorData> {
      const apiEditorData = yield this.architectApi.openDocumentationEditor(node.origamId);
      const editorData = new EditorData(apiEditorData, node);
      this.openEditor(editorData, 'DocumentationEditor');
    }.bind(this);
  }

  openDeploymentScriptsGeneratorEditor() {
    return function* (
      this: EditorTabViewState,
    ): Generator<Promise<IDatabaseResultResponse>, void, IDatabaseResultResponse> {
      const response = yield this.architectApi.fetchDeploymentScriptsList(null);

      const tempEditorData: IApiEditorData = {
        editorId: 'DeploymentScriptsGeneratorEditor-Id',
        editorType: 'DeploymentScriptsGeneratorEditor' as EditorType,
        parentNodeId: undefined,
        isDirty: false,
        node: {
          id: '',
          origamId: '',
          nodeText: '',
          editorType: 'DeploymentScriptsGeneratorEditor',
        },
        data: {
          possibleDeploymentVersions: response.deploymentVersions,
          currentDeploymentVersionId: response.currentDeploymentVersionId,
          results: response.results,
        },
      };

      const editorData = new EditorData(tempEditorData, null);
      this.openEditor(editorData, 'DeploymentScriptsGeneratorEditor');
    }.bind(this);
  }

  openEditor(editorData: EditorData, editorType?: EditorType) {
    const alreadyOpenEditor = this.editorsContainers.find(
      editor => editor.state.editorId === editorData.editorId,
    );
    if (alreadyOpenEditor) {
      this.setActiveEditor(alreadyOpenEditor.state.editorId);
      return;
    }

    const editor = getEditorContainer({
      editorType: editorType === undefined ? editorData.editorType : editorType,
      editorData: editorData,
      propertiesState: this.rootStore.propertiesState,
      architectApi: this.architectApi,
      modelTreeState: this.rootStore.modelTreeState,
      runGeneratorHandled: this.runGeneratorHandled,
    });
    if (!editor) {
      return;
    }

    this.editorsContainers.push(editor);
    this.setActiveEditor(editor.state.editorId);
  }

  get activeEditorState() {
    return this.editorsContainers.find(editor => editor.state.isActive)?.state;
  }

  setActiveEditor(schemaItemId: string) {
    for (const editor of this.editorsContainers) {
      editor.state.isActive = editor.state.editorId === schemaItemId;
    }
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

      this.editorsContainers = this.editorsContainers.filter(
        (editor: EditorContainer) => editor.state.editorId !== editorId,
      );

      if (editorId !== 'DeploymentScriptsGeneratorEditor-Id') {
        yield this.architectApi.closeEditor(editorId);
      }

      yield this.architectApi.closeEditor(editorId);

      if (this.editorsContainers.length > 0) {
        const editorToActivate = this.editorsContainers[this.editorsContainers.length - 1];
        this.setActiveEditor(editorToActivate.state.editorId);
      }
    }.bind(this);
  }
}

export interface IEditorNode extends IApiEditorNode {
  parent: TreeNode | null;
}
