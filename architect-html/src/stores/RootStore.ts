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
import { observable } from "mobx";
import { IApiTreeNode, IArchitectApi } from "src/API/IArchitectApi.ts";
import { TreeNode } from "src/components/modelTree/TreeNode.ts";
import {
  EditorTabViewState
} from "src/components/editorTabView/EditorTabViewState.ts";
import { TabViewState } from "src/components/tabView/TabViewState.ts";
import { TreeViewUiState } from "src/stores/TreeViewUiState.ts";
import { PackagesState } from "src/components/packages/PackagesState.ts";

export class RootStore {
  public projectState: ProjectState;

  constructor() {
    const architectApi = new ArchitectApi();
    this.projectState = new ProjectState(architectApi);
  }
}

export interface IModelNodesContainer {
  modelNodes: TreeNode[];

  loadPackageNodes(): Generator<Promise<IApiTreeNode[]>, void, IApiTreeNode[]>;

  findNodeById(nodeId: string | undefined): TreeNode | null;
}

export class ProjectState implements IModelNodesContainer {
  @observable accessor modelNodes: TreeNode[] = []

  public editorTabViewState: EditorTabViewState;
  public sideBarTabViewState = new TabViewState();
  public treeViewUiState = new TreeViewUiState();
  public packagesState: PackagesState

  constructor(private architectApi: IArchitectApi) {
    this.packagesState = new PackagesState(architectApi);
    this.editorTabViewState = new EditorTabViewState(architectApi, this);
  }
  showModelTree() {
    this.sideBarTabViewState.activeTabIndex = 1;
  }

  * loadPackageNodes(): Generator<Promise<IApiTreeNode[]>, void, IApiTreeNode[]> {
    const apiNodes = yield this.architectApi.getTopModelNodes();
    this.modelNodes = apiNodes.map(node =>
      new TreeNode(node, this.editorTabViewState, this.architectApi, this.treeViewUiState))
  }

  findNodeById(nodeId: string | undefined): TreeNode | null {
    return this.findNodeByIdRecursively(nodeId, this.modelNodes);
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
}


