import { ICaptionPosition } from "./ICaptionPosition";
import { IPropertyColumn } from "./IPropertyColumn";
import { IDropDownColumn } from "./IDropDownColumn";

export interface IPropertyData {
  id: string;
  modelInstanceId: string;
  name: string;
  readOnly: boolean;
  x: number;
  y: number;
  width: number;
  height: number;
  captionLength: number;
  captionPosition?: ICaptionPosition;
  entity: string;
  column: IPropertyColumn;
  dock?: string;
  multiline: boolean;
  isPassword: boolean;
  isRichText: boolean;
  maxLength: number;
  dataIndex: number;
  formatterPattern: string;

  dropDownShowUniqueValues?: boolean;
  lookupId?: string;
  identifier?: string;
  identifierIndex?: number;
  dropDownType?: string;
  cached?: boolean;
  searchByFirstColumnOnly?: boolean;
  allowReturnToForm?: boolean;
  isTree?: boolean;
  dropDownColumns: IDropDownColumn[];
}

export interface IProperty extends IPropertyData {
  $type_IProperty: 1;

  dataSourceIndex: number;
  lookupCache: Map<string, any>;
  parent?: any;
}

export const isIProperty = (o: any): o is IProperty => o.$type_IProperty;
