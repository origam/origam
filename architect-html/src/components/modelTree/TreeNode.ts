import {
  IApiTreeNode,
  EditorType, IArchitectApi,
  IMenuItemInfo
} from "src/API/IArchitectApi.ts";
import { action, flow, observable } from "mobx";
import {
  EditorProperty
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { NewEditorNode } from "src/components/modelTree/NewEditorNode.ts";
import {
  EditorTabViewState, IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import { TreeViewUiState } from "src/stores/TreeViewUiState.ts";

export class TreeNode implements IEditorNode {

  constructor(
    apiNode: IApiTreeNode,
    private editorManager: EditorTabViewState,
    private architectApi: IArchitectApi,
    private treeViewUiState: TreeViewUiState,
    public parent: TreeNode | null = null
  ) {
    this.id = apiNode.id;
    this.origamId = apiNode.origamId;
    this.nodeText = apiNode.nodeText;
    this.hasChildNodes = apiNode.hasChildNodes;
    this.isNonPersistentItem = apiNode.isNonPersistentItem;
    this.editorType = apiNode.editorType;
    this.childrenIds = apiNode.childrenIds;
    this.children = apiNode.children
      ? apiNode.children.map(child =>
        new TreeNode(child, this.editorManager, architectApi, this.treeViewUiState, this))
      : [];
  }

  id: string;
  origamId: string;
  nodeText: string;
  hasChildNodes: boolean;
  isNonPersistentItem: boolean;
  editorType: EditorType;
  children: TreeNode[];
  childrenIds: string[];

  @observable accessor isLoading: boolean = false;
  @observable accessor contextMenuItems: IMenuItemInfo[] = [];

  get isExpanded() {
    return this.treeViewUiState.isExpanded(this.id);
  }

  * loadChildren(): Generator<Promise<IApiTreeNode[]>, void, IApiTreeNode[]> {
    if (this.isLoading) {
      return;
    }
    if (!this.hasChildNodes) {
      this.children = [];
      return;
    }
    this.isLoading = true;
    try {
      const nodes = yield this.architectApi.getNodeChildren(this);
      this.children = nodes.map(node =>
        new TreeNode(node, this.editorManager, this.architectApi, this.treeViewUiState, this));
    } finally {
      this.isLoading = false;
    }
  }

  * toggle() {
    if (this.hasChildNodes && !this.isLoading && !this.isExpanded && (this.children.length === 0)) { // !isExpanded => will be expanded now
      yield flow(this.loadChildren.bind(this))();
    }
    this.treeViewUiState.setExpanded(this.id, !this.isExpanded)
  }

  * delete() {
    yield this.architectApi.deleteSchemaItem(this.origamId);
    if (this.parent) {
      yield* this.parent.loadChildren.bind(this.parent)();
    }
  }

  @action
  async getMenuItems() {
    this.contextMenuItems = await this.architectApi.getMenuItems(this);
  }

  async createNew(typeName: string) {
    const editorData = await this.architectApi.createNew(this, typeName);
    const properties = editorData.properties.map(property => new EditorProperty(property));
    const editorNode= new NewEditorNode(editorData.node, this);
    this.editorManager.openEditor(editorNode, properties);
  }
}



