import { IProperty } from './IProperty';

export const CDataTable = "CDataTable";

export interface IDataTableData {

}

export interface IDataTable extends IDataTableData {
  properties: IProperty[];
  rows: any[][];

  setRecords(rows: any[][]): void;

  parent?: any;
}