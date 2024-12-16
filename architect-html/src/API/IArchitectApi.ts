export interface IArchitectApi {

  getOpenEditors(): Promise<IApiEditorData[]>;

  getPackages(): Promise<IPackagesInfo>;

  setActivePackage(packageId: string): Promise<void>;

  getTopModelNodes(): Promise<IApiTreeNode[]>;

  getNodeChildren(node: INodeLoadData): Promise<IApiTreeNode[]>;

  openEditor(schemaItemId: string): Promise<IApiEditorData>;

  closeEditor(schemaItemId: string): Promise<void>

  persistChanges(schemaItemId: string): Promise<void>;

  updateProperties(
    schemaItemId: string | undefined,
    changes: IPropertyChange[]
  ): Promise<IUpdatePropertiesResult>;

  updateScreenEditor(args: {
    schemaItemId: string | undefined,
    name: string,
    selectedDataSourceId: string
    modelChanges: IModelChange[]
  }): Promise<ISectionEditorModel>;

  deleteSchemaItem(schemaItemId: string): Promise<void>;

  getMenuItems(node: INodeLoadData): Promise<IMenuItemInfo[]>;

  createNode(node: INodeLoadData, typeName: string): Promise<IApiEditorData>;

  createScreenEditorItem(args:{
    editorSchemaItemId: string,
    parentControlSetItemId: string,
    componentType: string,
    fieldName?: string,
    top: number,
    left: number
  }) : Promise<ApiControl>

  deleteScreenEditorItem(args:{
    schemaItemId: string,
    editorSchemaItemId: string,
  }) : Promise<ISectionEditorModel>
}

export interface IModelChange {
  schemaItemId: string;
  parentSchemaItemId: string | undefined;
  changes: IPropertyChange[]
}

export interface IPropertyChange {
  name: string;
  controlPropertyId: string | null;
  value: string;
}

export interface ISectionEditorModel {
  data: ISectionEditorData,
  isDirty: boolean;
}

export interface ISectionEditorData {
  dataSources: IDataSource[];
  name: string;
  schemaExtensionId: string;
  rootControl: ApiControl;
  selectedDataSourceId: string;
  fields: IEditorField[];
}

export interface ApiControl {
  name: string;
  id: string;
  type: string;
  properties: IApiEditorProperty[];
  children: ApiControl[];
}

export interface IUpdatePropertiesResult {
  propertyUpdates: IPropertyUpdate[];
  isDirty: boolean;
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

export type EditorType =
  "GridEditor"
  | "XslTEditor"
  | "ScreenSectionEditor"
  | null;

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

export interface IPackagesInfo {
  packages: IPackage[];
  activePackageId: string;
}

export interface IPackage {
  id: string
  name: string
}

export type PropertyType =
  "boolean"
  | "enum"
  | "string"
  | "integer"
  | "float"
  | "looukup";

export interface IApiEditorProperty {
  name: string;
  controlPropertyId: string | null;
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
  node: IApiEditorNode;
  data: IApiEditorProperty[] | ISectionEditorData;
  isDirty: boolean;
}

export interface IApiEditorNode {
  id: string;
  origamId: string;
  nodeText: string;
  editorType: EditorType;
}