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
import { Package } from "src/API/IArchitectApi.ts";
import { TreeNode } from "src/components/lazyLoadedTree/TreeNode.ts";
import { UiStore } from "src/stores/UiStore.ts";
import { Editor, getEditor } from "src/components/editors/GetEditor.tsx";

export class RootStore {
  public projectState: ProjectState;

  constructor(uiStore: UiStore) {
    const architectApi = new ArchitectApi();
    this.projectState = new ProjectState(architectApi, uiStore);
  }
}

export class ProjectState {
  @observable.ref accessor packages: Package[] = [];
  @observable accessor activePackageId: string | undefined;
  @observable accessor modelNodes: TreeNode[] = []
  @observable accessor editorStates: Editor[] = [];

  constructor(private architectApi: ArchitectApi, private uiStore: UiStore) {
  }

  * loadPackages(): Generator<Promise<Package[]>, void, Package[]> {
    this.packages = yield this.architectApi.getPackages();
  }

  * setActivePackage(packageId: string) {
    yield this.architectApi.setActivePackage(packageId);
    this.activePackageId = packageId;
  }

  * loadPackageNodes(): Generator<Promise<TreeNode[]>, void, TreeNode[]> {
    const apiNodes = yield this.architectApi.getTopModelNodes();
    this.modelNodes = apiNodes.map(node => new TreeNode(node, this.architectApi, this.uiStore.treeViewUiState))
  }

  @action.bound
  openEditor(node: TreeNode) {
    const editor = getEditor(node, this.architectApi);
    if (!editor) {
      return;
    }
    editor.state.isActive = true;
    this.editorStates = [editor]; //will be changed when we implement tabs
  }

  get activeEditorState() {
    return this.editorStates.find(editor => editor.state.isActive)?.state;
  }

  get activeEditor() {
    return this.editorStates.find(editor => editor.state.isActive)?.element;
  }
}



