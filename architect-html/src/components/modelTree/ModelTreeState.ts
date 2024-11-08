import { observable } from "mobx";
import { TreeNode } from "src/components/modelTree/TreeNode.ts";
import { IApiTreeNode, IArchitectApi } from "src/API/IArchitectApi.ts";
import { RootStore } from "src/stores/RootStore.ts";

export class ModelTreeState {
  @observable accessor modelNodes: TreeNode[] = [];

  constructor(
    private architectApi: IArchitectApi,
    private rootStore: RootStore) {
  }

  * loadPackageNodes(): Generator<Promise<IApiTreeNode[]>, void, IApiTreeNode[]> {
    const apiNodes = yield this.architectApi.getTopModelNodes();
    this.modelNodes = apiNodes.map(node =>
      new TreeNode(
        node,
        this.rootStore.getEditorTabViewState(),
        this.architectApi,
        this.rootStore.getUiState()))
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