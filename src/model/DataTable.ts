import { IDataTable, IDataTableData } from "./types/IDataTable";
import { observable, action, computed } from "mobx";
import { IProperty } from "./types/IProperty";
import { getDataView } from "./selectors/DataView/getDataView";
import { IAdditionalRowData } from "./types/IAdditionalRecordData";
import { AdditionalRowData } from "./AdditionalRowData";

export class DataTable implements IDataTable {
  $type_IDataTable: 1 = 1;

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
      if (ard.dirtyFormValues.has(property.id)) {
        return ard.dirtyFormValues.get(property.id);
      }
      if (ard.dirtyValues.has(property.id)) {
        return ard.dirtyValues.get(property.id);
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

  getAdditionalRowData(row: any[]) {
    return this.additionalRowData.get(this.getRowId(row));
  }

  hasRowDirtyValues(row: any[]) {
    const ard = this.getAdditionalRowData(row);
    if (!ard) return;
    return ard.dirtyValues.size > 0;
  }

  isRowDirtyNew(row: any[]) {
    const ard = this.getAdditionalRowData(row);
    return ard && ard.dirtyNew;
  }

  isRowDirtyDeleted(row: any[]) {
    const ard = this.getAdditionalRowData(row);
    return ard && ard.dirtyDeleted;
  }

  getDirtyValues(row: any[]): Map<string, any> {
    const ard = this.getAdditionalRowData(row);
    if (ard) {
      return ard.dirtyValues;
    } else {
      return new Map();
    }
  }

  getDirtyValueRows(): any[][] {
    return this.rows.filter(row => this.hasRowDirtyValues(row));
  }

  getDirtyDeletedRows(): any[][] {
    return this.rows.filter(row => this.isRowDirtyDeleted(row));
  }

  getDirtyNewRows(): any[][] {
    return this.rows.filter(row => this.isRowDirtyNew(row));
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
  flushFormToTable(row: any[]): void {
    const ard = this.getAdditionalRowData(row);
    if (ard) {
      for (let [propertyId, value] of ard.dirtyFormValues.entries()) {
        ard.dirtyValues.set(propertyId, value);
      }
    }
  }

  @action.bound
  setDirtyDeleted(row: any[]): void {
    this.createAdditionalData(row);
    this.getAdditionalRowData(row)!.dirtyDeleted = true;
  }

  @action.bound
  setDirtyNew(row: any[]): void {
    this.createAdditionalData(row);
    this.getAdditionalRowData(row)!.dirtyNew = true;
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
    this.additionalRowData.clear();
  }

  parent?: any;
}
