import { ApiTreeNode } from "src/API/IArchitectApi.ts";
import { flow, observable } from "mobx";
import { ArchitectApi } from "src/API/ArchitectApi.ts";
import { TreeViewUiState } from "src/stores/UiStore.ts";

export class TreeNode {

  constructor(
    apiNode: ApiTreeNode,
    private architectApi: ArchitectApi,
    private treeViewUiState: TreeViewUiState,
    private parent: TreeNode | null = null
  ) {
    this.id = apiNode.id;
    this.origamId = apiNode.origamId;
    this.nodeText = apiNode.nodeText;
    this.hasChildNodes = apiNode.hasChildNodes;
    this.isNonPersistentItem = apiNode.isNonPersistentItem;
    this.editorType = apiNode.editorType;
    this.childrenIds = apiNode.childrenIds;
    this.children = apiNode.children
      ? apiNode.children.map(child => new TreeNode(child, architectApi, this.treeViewUiState, this))
      : [];
  }

  id: string;
  origamId: string;
  nodeText: string;
  hasChildNodes: boolean;
  isNonPersistentItem: boolean;
  editorType: null | "GridEditor" | "XslTEditor";
  children: TreeNode[];
  childrenIds: string[];

  @observable accessor isLoading: boolean = false;

  get isExpanded() {
    return this.treeViewUiState.isExpanded(this.id);
  }

  * loadChildren() {
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
      this.children = nodes.map(node => new TreeNode(node, this.architectApi, this.treeViewUiState, this));
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
}



