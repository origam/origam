
export interface IArchitectApi {

  getOpenEditors():  Promise<IApiEditorData[]>;

  getPackages(): Promise<IPackage[]>;

  setActivePackage(packageId: string): Promise<void>;

  getTopModelNodes(): Promise<IApiTreeNode[]>;

  getNodeChildren(node: INodeLoadData): Promise<IApiTreeNode[]>;

  openEditor(schemaItemId: string): Promise<IApiEditorData>;

  closeEditor(schemaItemId: string): Promise<void>

  persistChanges(schemaItemId: string, changedProperties:  IApiEditorProperty[]): Promise<void>;

  updateProperties(
    schemaItemId: string | undefined,
    changedProperties: IApiEditorProperty[]
  ): Promise<IPropertyUpdate[]>;

  deleteSchemaItem(schemaItemId: string): Promise<void>;

  getMenuItems(node: INodeLoadData): Promise<IMenuItemInfo[]>;

  createNode(node: INodeLoadData, typeName: string): Promise<IApiEditorData>;
}

export interface ISectionEditorData {
  dataSources: IDataSource[];
  name: string;
  schemaExtensionId: string;
  selectedDataSource: string;
  id: string;
  fields: IEditorField[];
}

export interface IDataSource {
  schemaItemId: string;
  name: string;
}

export interface IEditorField {
  type: OrigamDataType;
  name: string;
}

export enum OrigamDataType {
  Boolean = 'Boolean',
  Blob = 'Blob',
  Byte = 'Byte',
  Currency = 'Currency',
  Date = 'Date',
  Long = 'Long',
  Memo = 'Memo',
  Float = 'Float',
  Integer = 'Integer',
  String = 'String',
  UniqueIdentifier = 'UniqueIdentifier',
  Xml = 'Xml',
  Array = 'Array',
  Geography = 'Geography',
  Object = 'Object'
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

export type EditorType = "GridEditor" | "XslTEditor" | "ScreenSectionEditor" | null;

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

export type PropertyType = "boolean" | "enum" | "string" | "integer" | "float" | "looukup";

export interface IApiEditorProperty {
  name: string;
  type: PropertyType;
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

export interface IApiEditorData {
  parentNodeId: string | undefined;
  isPersisted: boolean;
  node: IApiEditorNode;
  data: IApiEditorProperty[] | ISectionEditorData;
  // screenSectionEditorData: ISectionEditorData; ???
  // or something more generic to replace the properties?
}

export interface IApiEditorNode {
  id: string;
  origamId: string;
  nodeText: string;
  editorType: EditorType;
}