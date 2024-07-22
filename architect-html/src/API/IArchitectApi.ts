import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import { EditorProperty } from "src/components/gridEditor/GridEditor.tsx";

export interface IArchitectApi {
  getTopModelNodes(): Promise<TreeNode[]>;

  getNodeChildren(node: TreeNode): Promise<TreeNode[]>;

  getProperties(nodeId: string): Promise<EditorProperty[]>;
}