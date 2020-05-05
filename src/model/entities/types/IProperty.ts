import { ICaptionPosition } from "./ICaptionPosition";
import { IPropertyColumn } from "./IPropertyColumn";
import { IDropDownColumn } from "./IDropDownColumn";
import { ILookup } from "./ILookup";

export enum IDockType {
  Dock = "Dock",
  Fill = "Fill"
}

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
  dock?: IDockType;
  multiline: boolean;
  isPassword: boolean;
  isRichText: boolean;
  maxLength: number;
  gridColumnWidth: number;
  columnWidth: number;
  formatterPattern: string;
  customNumericFormat?: string;
  parameters?: any;
  allowReturnToForm?: boolean;
  isTree?: boolean;

  identifier?: string;
  lookup?: ILookup;
}

export interface IProperty extends IPropertyData {
  $type_IProperty: 1;

  dataSourceIndex: number;
  dataIndex: number;
  isLookup: boolean;
  
  linkToMenuId?: string;
  isLink: boolean;
  nameOverride: string | null | undefined;

  setColumnWidth(width: number): void;

  parent?: any;
}

export const isIProperty = (o: any): o is IProperty => o.$type_IProperty;
