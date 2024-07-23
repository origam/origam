import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import { EditorProperty, PropertyChange } from "src/components/gridEditor/GridEditor.tsx";

export interface IArchitectApi {
  getTopModelNodes(): Promise<TreeNode[]>;

  getNodeChildren(node: TreeNode): Promise<TreeNode[]>;

  getProperties(schemaItemId: string): Promise<EditorProperty[]>;

  persistChanges(schemaItemId: string, changedProperties:  EditorProperty[]): Promise<void>;
}