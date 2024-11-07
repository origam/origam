
export interface IArchitectApi {

  getPackages(): Promise<Package[]>;

  setActivePackage(packageId: string): Promise<void>;

  getTopModelNodes(): Promise<ApiTreeNode[]>;

  getNodeChildren(node: ApiTreeNode): Promise<ApiTreeNode[]>;

  openEditor(schemaItemId: string): Promise<ApiEditorProperty[]>;

  persistChanges(schemaItemId: string, changedProperties:  ApiEditorProperty[]): Promise<void>;

  checkRules(
    schemaItemId: string | undefined,
    parentId: string | undefined,
    fullTypeName: string | undefined,
    changedProperties: ApiEditorProperty[]
  ): Promise<RuleErrors[]>;

  deleteSchemaItem(schemaItemId: string): Promise<void>;

  getMenuItems(node: ApiTreeNode): Promise<MenuItemInfo[]>;

  createNew(node: ApiTreeNode, typeName: string): Promise<INewEditorData>;
}

export interface RuleErrors
{
  name: string;
  errors: string[];
}

export interface MenuItemInfo {
    caption: string;
    typeName: string;
    iconName: string;
    iconIndex: number | null;
}

export type EditorType = "GridEditor" | "XslTEditor" | null;

export interface ApiTreeNode {
  id: string;
  origamId: string;
  nodeText: string;
  hasChildNodes: boolean;
  isNonPersistentItem: boolean;
  editorType: EditorType;
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
  errors: string[];
}

export interface DropDownValue {
    name: string;
    value: any;
}

export interface INewEditorData {
  node: IApiEditorNode;
  properties: ApiEditorProperty[];
}

export interface IApiEditorNode {
  id: string;
  origamId: string;
  nodeText: string;
  editorType: EditorType;
}