export interface IArchitectApi {

  getPackages(): Promise<Package[]>;

  setActivePackage(packageId: string): Promise<void>;

  getTopModelNodes(): Promise<ApiTreeNode[]>;

  getNodeChildren(node: ApiTreeNode): Promise<ApiTreeNode[]>;

  getProperties(schemaItemId: string): Promise<ApiEditorProperty[]>;

  persistChanges(schemaItemId: string, changedProperties:  ApiEditorProperty[]): Promise<void>;

  deleteSchemaItem(schemaItemId: string): Promise<void>;
}

export interface ApiTreeNode {
  id: string;
  origamId: string;
  nodeText: string;
  hasChildNodes: boolean;
  isNonPersistentItem: boolean;
  editorType: null | "GridEditor" | "XslTEditor";
  childrenIds: string[];
  children?: ApiTreeNode[];
}

export interface Package {
  id: string
  name: string
}

export interface ApiEditorProperty {
  name: string;
  type: "boolean" | "enum" | "string" | "looukup";
  value: any;
  dropDownValues: DropDownValue[];
  category: string | null;
  description: string;
  readOnly: boolean;
}

export interface DropDownValue {
    name: string;
    value: any;
}