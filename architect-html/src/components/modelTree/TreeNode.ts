import {
  IApiTreeNode,
  EditorType, IArchitectApi,
  IMenuItemInfo, IApiEditorData
} from "src/API/IArchitectApi.ts";
import { observable } from "mobx";
import {
  EditorData,
} from "src/components/modelTree/EditorData.ts";
import { IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import { RootStore } from "src/stores/RootStore.ts";

export class TreeNode implements IEditorNode {
    private architectApi: IArchitectApi;

  constructor(
    apiNode: IApiTreeNode,
    private rootStore: RootStore,
    public parent: TreeNode | null = null
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
      ? apiNode.children.map(child =>
        new TreeNode(child, this.rootStore, this))
      : [];
    this.childrenInitialized = apiNode.hasChildNodes && this.children.length > 0
      || !apiNode.hasChildNodes;
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

  * loadChildren(): Generator<Promise<IApiTreeNode[]>, void, IApiTreeNode[]> {
    if (this.isLoading) {
      return;
    }
    this.isLoading = true;
    try {
      const nodes = yield this.architectApi.getNodeChildren(this);
      this.children = nodes.map(node =>
        new TreeNode(node, this.rootStore, this));
      this.childrenInitialized = true;
    } finally {
      this.isLoading = false;
    }
  }

  * toggle() {
    this.rootStore.uiState.setExpanded(this.id, !this.isExpanded);
    yield;
  }

  * delete() {
    yield this.architectApi.deleteSchemaItem(this.origamId);
    if (this.parent) {
      yield* this.parent.loadChildren.bind(this.parent)();
    }
  }

  * getMenuItems() {
    this.contextMenuItems = yield this.architectApi.getMenuItems(this);
  }

  createNode(typeName: string) {
    return function * (this: TreeNode): Generator<Promise<IApiEditorData>, void, IApiEditorData> {
      const apiEditorData = yield this.architectApi.createNode(this, typeName);
      const editorData = new EditorData(apiEditorData, this);
      this.rootStore.editorTabViewState.openEditor(editorData);
    }.bind(this);
  }
}



