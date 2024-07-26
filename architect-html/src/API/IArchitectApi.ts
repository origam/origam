import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import { EditorProperty } from "src/components/editors/gridEditor/GrirEditorSlice.ts";
import { Package } from "src/components/packages/Packages.tsx";

export interface IArchitectApi {

  getPackages(): Promise<Package[]>;

  setActivePackage(packageId: string): Promise<void>;

  getTopModelNodes(): Promise<TreeNode[]>;

  getNodeChildren(node: TreeNode): Promise<TreeNode[]>;

  getProperties(schemaItemId: string): Promise<EditorProperty[]>;

  persistChanges(schemaItemId: string, changedProperties:  EditorProperty[]): Promise<void>;
}