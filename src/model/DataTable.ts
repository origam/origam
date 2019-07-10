import { IDataTable, IDataTableData } from "./types/IDataTable";
import { observable, action, computed } from "mobx";
import { IProperty } from "./types/IProperty";
import { getDataView } from "./selectors/DataView/getDataView";

export class DataTable implements IDataTable {

  parent?: any;
  constructor(data: IDataTableData) {
    Object.assign(this, data);
  }

  @computed
  get properties(): IProperty[] {
    return getDataView(this).properties;
  }
  @observable.shallow rows: any[][] = [];

  getCellValue(row: any[], property: IProperty) {
    return row[property.dataIndex];
  }

  getRowByExistingIdx(idx: number): any[] {
    // TODO: Change to respect dirty deleted rows.
    return this.rows[idx];
  }

  getExistingRowIdxById(id: string) {
    const idx = this.rows.findIndex(row => row[0] === id);
    return idx > -1 ? idx : undefined;
  }

  getPropertyById(id: string) {
    return this.properties.find(prop => prop.id === id);
  }

  getFirstRow(): any[] | undefined {
    return this.rows[0];
  }

  @action.bound setRecords(rows: any[][]) {
    this.rows.length = 0;
    this.rows.push(...rows);
  }
}
