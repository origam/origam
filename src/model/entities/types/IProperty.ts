import { ICaptionPosition } from "./ICaptionPosition";
import { IPropertyColumn } from "./IPropertyColumn";
import { IDropDownColumn } from "./IDropDownColumn";
import { ILookup } from "./ILookup";

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
  
  allowReturnToForm?: boolean;
  isTree?: boolean;
  
  lookup?: ILookup;
  
}

export interface IProperty extends IPropertyData {
  $type_IProperty: 1;

  dataSourceIndex: number;
  isLookup: boolean;
  
  parent?: any;
}

export const isIProperty = (o: any): o is IProperty => o.$type_IProperty;
