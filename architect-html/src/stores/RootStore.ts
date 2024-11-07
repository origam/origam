/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

import { ArchitectApi } from "src/API/ArchitectApi.ts";
import { action, observable } from "mobx";
import {
  ApiTreeNode,
  IArchitectApi,
  IEditorData,
  Package
} from "src/API/IArchitectApi.ts";
import {
  TreeNode
} from "src/components/lazyLoadedTree/TreeNode.ts";
import { UiStore } from "src/stores/UiStore.ts";
import { Editor, getEditor } from "src/components/editors/GetEditor.tsx";
import { IEditorManager, IEditorNode } from "src/stores/IEditorManager.ts";
import {
  EditorProperty
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { NewEditorNode } from "src/components/lazyLoadedTree/NewEditorNode.ts";

export class RootStore {
  public projectState: ProjectState;

  constructor(uiStore: UiStore) {
    const architectApi = new ArchitectApi();
    this.projectState = new ProjectState(architectApi, uiStore);
  }
}

export class ProjectState implements IEditorManager {
  @observable.ref accessor packages: Package[] = [];
  @observable accessor activePackageId: string | undefined;
  @observable accessor modelNodes: TreeNode[] = []
  @observable accessor editors: Editor[] = [];

  constructor(private architectApi: IArchitectApi, private uiStore: UiStore) {
  }

  * loadPackages(): Generator<Promise<Package[]>, void, Package[]> {
    this.packages = yield this.architectApi.getPackages();
  }

  * setActivePackage(packageId: string) {
    yield this.architectApi.setActivePackage(packageId);
    this.activePackageId = packageId;
  }

  * loadPackageNodes(): Generator<Promise<ApiTreeNode[]>, void, ApiTreeNode[]> {
    const apiNodes = yield this.architectApi.getTopModelNodes();
    this.modelNodes = apiNodes.map(node =>
      new TreeNode(node, this, this.architectApi, this.uiStore.treeViewUiState))
  }

  private findNodeByIdRecursively(nodeId: string | undefined, nodes: TreeNode[]): TreeNode | null {
    if (!nodeId) {
      return null;
    }
    for (const node of nodes) {
      if (node.id === nodeId) {
        return node;
      }
      const foundNode = this.findNodeByIdRecursively(nodeId, node.children);
      if (foundNode) {
        return foundNode;
      }
    }
    return null;
  }

  * initializeOpenEditors(): Generator<Promise<IEditorData[]>, void, IEditorData[]> {
    const openEditorsData = (yield this.architectApi.getOpenEditors()) as IEditorData[];
    this.editors = openEditorsData
      .map(data => this.toEditor(data)) as Editor[];
    if(this.editors.length > 0){
      this.setActiveEditor(this.editors[this.editors.length - 1].state.schemaItemId);
    }
  }

  private toEditor(data: IEditorData) {
    const parentNode = this.findNodeByIdRecursively(data.parentNodeId, this.modelNodes)
    return getEditor(
      new NewEditorNode(data.node, parentNode),
        data.properties.map(property => new EditorProperty(property)),
        this.architectApi)
  }

  @action.bound
  openEditor(node: IEditorNode, properties?: EditorProperty[]): void {
    const alreadyOpenEditor = this.editors.find(editor => editor.state.schemaItemId === node.origamId);
    if (alreadyOpenEditor) {
      this.setActiveEditor(alreadyOpenEditor.state.schemaItemId);
      return;
    }

    const editor = getEditor(node, properties, this.architectApi);
    if (!editor) {
      return;
    }
    this.editors.push(editor)
    this.setActiveEditor(editor.state.schemaItemId);
  }

  get activeEditorState() {
    return this.editors.find(editor => editor.state.isActive)?.state;
  }

  get activeEditor() {
    return this.editors.find(editor => editor.state.isActive)?.element;
  }

  setActiveEditor(schemaItemId: string) {
    for (const editor of this.editors) {
      editor.state.isActive = editor.state.schemaItemId === schemaItemId;
    }
  }

  closeEditor(schemaItemId: string) {
  return function* (this: any) {
    this.editors = this.editors.filter(editor => editor.state.schemaItemId !== schemaItemId);
    yield this.architectApi.closeEditor(schemaItemId);
  }.bind(this);
}
}



