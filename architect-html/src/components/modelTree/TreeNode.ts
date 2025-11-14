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
  EditorSubType,
  IApiEditorData,
  IApiTreeNode,
  IArchitectApi,
  IMenuItemInfo,
} from '@api/IArchitectApi';
import { IEditorNode } from '@components/editorTabView/EditorTabViewState';
import { EditorData } from '@components/modelTree/EditorData';
import { RootStore } from '@stores/RootStore';
import { observable } from 'mobx';

export class TreeNode implements IEditorNode {
  private architectApi: IArchitectApi;

  constructor(
    apiNode: IApiTreeNode,
    private rootStore: RootStore,
    public parent: TreeNode | null = null,
  ) {
    this.architectApi = rootStore.architectApi;
    this.id = apiNode.id;
    this.origamId = apiNode.origamId;
    this.nodeText = apiNode.nodeText;
    this.isNonPersistentItem = apiNode.isNonPersistentItem;
    this.editorType = apiNode.defaultEditor;
    this.childrenIds = apiNode.childrenIds;
    this.iconUrl = apiNode.iconUrl;
    this.itemType = apiNode.itemType;
    this.isCurrentVersion = apiNode.isCurrentVersion;
    this.children = apiNode.children
      ? apiNode.children.map(child => new TreeNode(child, this.rootStore, this))
      : [];
    this.childrenInitialized =
      (apiNode.hasChildNodes && this.children.length > 0) || !apiNode.hasChildNodes;
  }

  id: string;
  origamId: string;
  nodeText: string;
  childrenInitialized: boolean;
  isNonPersistentItem: boolean;
  editorType: EditorSubType;
  children: TreeNode[];
  childrenIds: string[];
  iconUrl?: string;
  itemType?: string;
  isCurrentVersion?: boolean;

  @observable accessor isLoading: boolean = false;
  @observable accessor contextMenuItems: IMenuItemInfo[] = [];

  get isExpanded() {
    return this.rootStore.uiState.isExpanded(this.id);
  }

  get isDeploymentVersion() {
    return this.itemType === 'Origam.Schema.DeploymentModel.DeploymentVersion';
  }

  get isUpdateScriptActivity() {
    return this.itemType === 'Origam.Schema.DeploymentModel.ServiceCommandUpdateScriptActivity';
  }

  *loadChildren(): Generator<Promise<IApiTreeNode[]>, void, IApiTreeNode[]> {
    if (this.isLoading) {
      return;
    }
    this.isLoading = true;
    try {
      const nodes = yield this.architectApi.getNodeChildren(this);
      this.children = nodes.map(node => new TreeNode(node, this.rootStore, this));
      this.childrenInitialized = true;
    } finally {
      this.isLoading = false;
    }
  }

  *toggle() {
    this.rootStore.uiState.setExpanded(this.id, !this.isExpanded);
    yield;
  }

  *delete() {
    yield this.architectApi.deleteSchemaItem(this.origamId);
    if (this.parent) {
      yield* this.parent.loadChildren.bind(this.parent)();
    }
  }

  *getMenuItems() {
    this.contextMenuItems = yield this.architectApi.getMenuItems(this);
  }

  *setVersionCurrent() {
    yield this.architectApi.setVersionCurrent(this.origamId);
    if (this.parent) {
      yield* this.parent.loadChildren.bind(this.parent)();
    }
  }

  *runUpdateScriptActivity() {
    yield this.architectApi.runUpdateScriptActivity(this.origamId);
  }

  createNode(typeName: string) {
    return function* (this: TreeNode): Generator<Promise<IApiEditorData>, void, IApiEditorData> {
      const apiEditorData = yield this.architectApi.createNode(this, typeName);
      const editorData = new EditorData(apiEditorData, this);
      this.rootStore.editorTabViewState.openEditor(editorData);
    }.bind(this);
  }
}
