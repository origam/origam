import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import { EditorProperty } from "src/components/editors/gridEditor/GrirEditorSlice.ts";

export interface IArchitectApi {
  getTopModelNodes(): Promise<TreeNode[]>;

  getNodeChildren(node: TreeNode): Promise<TreeNode[]>;

  getProperties(schemaItemId: string): Promise<EditorProperty[]>;

  persistChanges(schemaItemId: string, changedProperties:  EditorProperty[]): Promise<void>;
}