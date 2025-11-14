/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

export interface IArchitectApi {
  getOpenEditors(): Promise<IApiEditorData[]>;

  getPackages(): Promise<IPackagesInfo>;

  setActivePackage(packageId: string): Promise<void>;

  getTopModelNodes(): Promise<IApiTreeNode[]>;

  getNodeChildren(node: INodeLoadData): Promise<IApiTreeNode[]>;

  openEditor(schemaItemId: string): Promise<IApiEditorData>;

  closeEditor(editorId: string): Promise<void>;

  persistChanges(schemaItemId: string): Promise<void>;

  persistSectionEditorChanges(schemaItemId: string): Promise<void>;

  updateProperties(
    schemaItemId: string | undefined,
    changes: IPropertyChange[],
  ): Promise<IUpdatePropertiesResult>;

  deleteSchemaItem(schemaItemId: string): Promise<void>;

  getMenuItems(node: INodeLoadData): Promise<IMenuItemInfo[]>;

  createNode(node: INodeLoadData, typeName: string): Promise<IApiEditorData>;

  updateSectionEditor(args: {
    schemaItemId: string | undefined;
    name: string;
    selectedDataSourceId: string;
    modelChanges: IModelChange[];
  }): Promise<ISectionEditorModel>;

  createSectionEditorItem(args: {
    editorSchemaItemId: string;
    parentControlSetItemId: string;
    componentType: string;
    fieldName?: string;
    top: number;
    left: number;
  }): Promise<IApiControl>;

  deleteSectionEditorItem(args: {
    schemaItemIds: string[];
    editorSchemaItemId: string;
  }): Promise<ISectionEditorModel>;

  updateScreenEditor(args: {
    schemaItemId: string | undefined;
    name: string;
    selectedDataSourceId: string;
    modelChanges: IModelChange[];
  }): Promise<IScreenEditorModel>;

  createScreenEditorItem(args: {
    editorSchemaItemId: string;
    parentControlSetItemId: string;
    controlItemId: string;
    top: number;
    left: number;
  }): Promise<IScreenEditorItem>;

  deleteScreenEditorItem(args: {
    schemaItemIds: string[];
    editorSchemaItemId: string;
  }): Promise<IScreenEditorModel>;

  loadSections(
    editorSchemaItemId: string,
    sectionIds: string[],
  ): Promise<Record<string, IApiControl>>;

  openDocumentationEditor(origamId: string): Promise<IApiEditorData>;

  updateDocumentationProperties(
    schemaItemId: string,
    changes: IPropertyChange[],
  ): Promise<IUpdatePropertiesResult>;

  persistDocumentationChanges(schemaItemId: string): Promise<void>;

  setVersionCurrent(schemaItemId: string): Promise<void>;
  runUpdateScriptActivity(schemaItemId: string): Promise<void>;
}
export interface IScreenEditorModel {
  data: IScreenEditorData;
  isDirty: boolean;
}

export interface IScreenEditorItem {
  screenItem: IApiControl;
  section: IApiControl;
}

export interface IModelChange {
  schemaItemId: string;
  parentSchemaItemId: string | undefined;
  changes: IPropertyChange[];
}

export interface IPropertyChange {
  name: string;
  controlPropertyId: string | null;
  value: string;
}

export interface ISectionEditorModel {
  data: ISectionEditorData;
  isDirty: boolean;
}

export interface IDesignerEditorData {
  dataSources: IDataSource[];
  name: string;
  schemaExtensionId: string;
  rootControl: IApiControl;
  selectedDataSourceId: string;
}

export interface ISectionEditorData extends IDesignerEditorData {
  fields: IEditorField[];
}

export interface IScreenEditorData extends IDesignerEditorData {
  sections: IToolBoxItem[];
  widgets: IToolBoxItem[];
}

export interface IToolBoxItem {
  id: string;
  name: string;
}

export interface IApiControl {
  name: string;
  id: string;
  type: string;
  properties: IApiEditorProperty[];
  children: IApiControl[];
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
  Object = 'Object',
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

export type EditorSubType =
  | 'DeploymentScriptsEditor'
  | 'GridEditor'
  | 'XsltEditor'
  | 'ScreenSectionEditor'
  | 'ScreenEditor'
  | null;

export type EditorType = EditorSubType | 'DocumentationEditor';

export interface INodeLoadData {
  id: string;
  nodeText: string;
  isNonPersistentItem: boolean;
}

export interface IApiTreeNode extends INodeLoadData {
  origamId: string;
  hasChildNodes: boolean;
  defaultEditor: EditorSubType;
  childrenIds: string[];
  children?: IApiTreeNode[];
  iconUrl?: string;
  itemType?: string;
  isCurrentVersion?: boolean;
}

export interface IPackagesInfo {
  packages: IPackage[];
  activePackageId: string;
}

export interface IPackage {
  id: string;
  name: string;
}

export type PropertyType = 'boolean' | 'enum' | 'string' | 'integer' | 'float' | 'looukup';

export interface DocumentationEditorData {
  label: string;
  properties: IApiEditorProperty[];
}

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
  editorId: string;
  editorType: EditorType;
  parentNodeId: string | undefined;
  node: IApiEditorNode;
  data: IApiEditorProperty[] | ISectionEditorData | IScreenEditorData | DocumentationEditorData;
  isDirty: boolean;
}

export interface IApiEditorNode {
  id: string;
  origamId: string;
  nodeText: string;
  editorType: EditorSubType;
}
