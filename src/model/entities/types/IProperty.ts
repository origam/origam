import { ICaptionPosition } from "./ICaptionPosition";
import { IPropertyColumn } from "./IPropertyColumn";
import { ILookup } from "./ILookup";
import { ILookupIndividualEngine } from "../Property";

export enum IDockType {
  Dock = "Dock",
  Fill = "Fill",
}

export interface IPropertyData {
  id: string;
  tabIndex: string | undefined;
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
  isAggregatedColumn: boolean;
  isLookupColumn: boolean;

  controlPropertyValue?: string;
  controlPropertyId?: string;
  parameters?: any;
  allowReturnToForm?: boolean;
  isTree?: boolean;
  style: any;
  identifier?: string;
  lookup?: ILookup;
  lookupId?: string;
  xmlNode: any;
}

export interface IProperty extends IPropertyData {
  $type_IProperty: 1;

  dataSourceIndex: number;
  dataIndex: number;
  isLookup: boolean;
  lookupEngine?: ILookupIndividualEngine;

  childProperties: IProperty[];
  linkToMenuId?: string;
  isLink: boolean;
  nameOverride: string | null | undefined;
  isFormField: boolean;

  getPolymophicProperty(row: any[]): IProperty;

  setColumnWidth(width: number): void;

  stop(): void;

  parent?: any;
}

export const isIProperty = (o: any): o is IProperty => o.$type_IProperty;
