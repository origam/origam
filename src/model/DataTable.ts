import { IDataTable, IDataTableData } from "./types/IDataTable";
import { observable, action, computed } from "mobx";
import { IProperty } from "./types/IProperty";
import { getDataView } from "./selectors/DataView/getDataView";
import { IAdditionalRowData } from "./types/IAdditionalRecordData";
import { AdditionalRowData } from "./AdditionalRowData";

export class DataTable implements IDataTable {
  constructor(data: IDataTableData) {
    Object.assign(this, data);
  }

  @observable.shallow rows: any[][] = [];
  @observable additionalRowData: Map<string, IAdditionalRowData> = new Map();

  @computed
  get properties(): IProperty[] {
    return getDataView(this).properties;
  }

  getRowId(row: any[]): string {
    return row[0];
  }

  getCellValue(row: any[], property: IProperty) {
    if (this.additionalRowData.has(this.getRowId(row))) {
      const ard = this.additionalRowData.get(this.getRowId(row))!;
      if(ard.dirtyFormValues.has(property.id)) {
        return ard.dirtyFormValues.get(property.id)
      }
      if(ard.dirtyValues.has(property.id)) {
        return ard.dirtyValues.get(property.id)
      }
    }
    return row[property.dataIndex];
  }

  getRowByExistingIdx(idx: number): any[] {
    // TODO: Change to respect dirty deleted rows.
    return this.rows[idx];
  }

  getExistingRowIdxById(id: string) {
    const idx = this.rows.findIndex(row => this.getRowId(row) === id);
    return idx > -1 ? idx : undefined;
  }

  getPropertyById(id: string) {
    return this.properties.find(prop => prop.id === id);
  }

  getFirstRow(): any[] | undefined {
    return this.rows[0];
  }

  @action.bound
  setRecords(rows: any[][]) {
    this.clear();
    this.rows.push(...rows);
  }

  @action.bound
  setFormDirtyValue(row: any[], propertyId: string, value: any) {
    this.createAdditionalData(row);
    this.additionalRowData
      .get(this.getRowId(row))!
      .dirtyFormValues.set(propertyId, value);
  }

  @action.bound
  setDirtyDeleted(row: any[]): void {
    this.createAdditionalData(row);
    this.additionalRowData.get(this.getRowId(row))!.dirtyDeleted = true;
  }

  @action.bound
  setDirtyNew(row: any[]): void {
    this.createAdditionalData(row);
    this.additionalRowData.get(this.getRowId(row))!.dirtyNew = true;
  }

  @action.bound
  createAdditionalData(row: any[]) {
    if (!this.additionalRowData.has(row[0])) {
      const ard = new AdditionalRowData();
      ard.parent = this;
      this.additionalRowData.set(row[0], ard);
    }
  }

  @action.bound
  clear(): void {
    this.rows.length = 0;
  }

  parent?: any;
}
