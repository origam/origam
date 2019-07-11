import { IProperty } from "./IProperty";
import { IAdditionalRowData } from "./IAdditionalRecordData";

export const CDataTable = "CDataTable";

export interface IDataTableData {}

export interface IDataTable extends IDataTableData {
  properties: IProperty[];
  rows: any[][];
  additionalRowData: Map<string, IAdditionalRowData>;

  getCellValue(row: any[], property: IProperty): any;
  getRowByExistingIdx(idx: number): any[];
  getExistingRowIdxById(id: string): number | undefined;
  getPropertyById(id: string): IProperty | undefined;
  getFirstRow(): any[] | undefined;

  setRecords(rows: any[][]): void;
  setFormDirtyValue(row: any[], propertyId: string, value: any): void;
  setDirtyDeleted(row: any[]): void;
  setDirtyNew(row: any[]): void;
  clear(): void;
  parent?: any;
}
