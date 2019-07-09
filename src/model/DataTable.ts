import { IDataTable } from "./types/IDataTable";
import { observable, action } from "mobx";
import { IProperty } from "./types/IProperty";

export class DataTable implements IDataTable {
  properties: IProperty[] = [];
  @observable.shallow rows: any[][] = []

  @action.bound setRecords(rows: any[][]) {
    this.rows.length = 0;
    this.rows.push(...rows);
  }
}