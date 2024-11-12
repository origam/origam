
export interface IArchitectApi {

  getOpenEditors():  Promise<IEditorData[]>;

  getPackages(): Promise<IPackage[]>;

  setActivePackage(packageId: string): Promise<void>;

  getTopModelNodes(): Promise<IApiTreeNode[]>;

  getNodeChildren(node: INodeLoadData): Promise<IApiTreeNode[]>;

  openEditor(schemaItemId: string): Promise<IApiEditorProperty[]>;

  closeEditor(schemaItemId: string): Promise<void>

  persistChanges(schemaItemId: string, changedProperties:  IApiEditorProperty[]): Promise<void>;

  updateProperties(
    schemaItemId: string | undefined,
    changedProperties: IApiEditorProperty[]
  ): Promise<IPropertyUpdate[]>;

  deleteSchemaItem(schemaItemId: string): Promise<void>;

  getMenuItems(node: INodeLoadData): Promise<IMenuItemInfo[]>;

  createNew(node: INodeLoadData, typeName: string): Promise<IEditorData>;
}


export interface IPropertyUpdate {
    propertyName: string;
    errors: string[];
    dropDownValues: IDropDownValue[];
}

export interface IMenuItemInfo {
    caption: string;
    typeName: string;
    iconName: string;
    iconIndex: number | null;
}

export type EditorType = "GridEditor" | "XslTEditor" | null;

export interface INodeLoadData {
  id: string;
  nodeText: string;
  isNonPersistentItem: boolean;
}

export interface IApiTreeNode extends INodeLoadData {
  origamId: string;
  hasChildNodes: boolean;
  editorType: EditorType;
  childrenIds: string[];
  children?: IApiTreeNode[];
}

export interface IPackage {
  id: string
  name: string
}

export interface IApiEditorProperty {
  name: string;
  type: "boolean" | "enum" | "string" | "looukup";
  value: any;
  dropDownValues: IDropDownValue[];
  category: string | null;
  description: string;
  readOnly: boolean;
  errors: string[];
}

export interface IDropDownValue {
    name: string;
    value: any;
}

export interface IEditorData {
  parentNodeId: string | undefined;
  isPersisted: boolean;
  node: IApiEditorNode;
  properties: IApiEditorProperty[];
}

export interface IApiEditorNode {
  id: string;
  origamId: string;
  nodeText: string;
  editorType: EditorType;
}