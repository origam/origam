import { EditorProperty } from "src/components/editors/gridEditor/GrirEditorSlice.ts";

export interface IArchitectApi {

  getPackages(): Promise<Package[]>;

  setActivePackage(packageId: string): Promise<void>;

  getTopModelNodes(): Promise<ApiTreeNode[]>;

  getNodeChildren(node: ApiTreeNode): Promise<ApiTreeNode[]>;

  getProperties(schemaItemId: string): Promise<EditorProperty[]>;

  persistChanges(schemaItemId: string, changedProperties:  EditorProperty[]): Promise<void>;

  deleteSchemaItem(schemaItemId: string): Promise<void>;
}

export interface ApiTreeNode {
  id: string;
  origamId: string;
  nodeText: string;
  hasChildNodes: boolean;
  isNonPersistentItem: boolean;
  editorType: null | "GridEditor";
  childrenIds: string[];
  children?: ApiTreeNode[];
}

export interface Package {
  id: string
  name: string
}
