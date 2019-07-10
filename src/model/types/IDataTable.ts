import { IProperty } from './IProperty';

export const CDataTable = "CDataTable";

export interface IDataTableData {}

export interface IDataTable extends IDataTableData {
  properties: IProperty[];
  rows: any[][];

  getCellValue(row: any[], property: IProperty): any;
  getRowByExistingIdx(idx: number): any[];
  getPropertyById(id: string): IProperty | undefined;

  setRecords(rows: any[][]): void;
  parent?: any;
}