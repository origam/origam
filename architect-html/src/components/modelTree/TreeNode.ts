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
  IApiTreeNode,
  EditorType,
  IArchitectApi,
  IMenuItemInfo,
  IApiEditorData,
} from 'src/API/IArchitectApi.ts';
import { observable } from 'mobx';
import { EditorData } from 'src/components/modelTree/EditorData.ts';
import { IEditorNode } from 'src/components/editorTabView/EditorTabViewState.ts';
import { RootStore } from 'src/stores/RootStore.ts';

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
    this.editorType = apiNode.editorType;
    this.childrenIds = apiNode.childrenIds;
    this.iconUrl = apiNode.iconUrl;
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
  editorType: EditorType;
  children: TreeNode[];
  childrenIds: string[];
  iconUrl?: string;

  @observable accessor isLoading: boolean = false;
  @observable accessor contextMenuItems: IMenuItemInfo[] = [];

  get isExpanded() {
    return this.rootStore.uiState.isExpanded(this.id);
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

  createNode(typeName: string) {
    return function* (this: TreeNode): Generator<Promise<IApiEditorData>, void, IApiEditorData> {
      const apiEditorData = yield this.architectApi.createNode(this, typeName);
      const editorData = new EditorData(apiEditorData, this);
      this.rootStore.editorTabViewState.openEditor(editorData);
    }.bind(this);
  }
}
