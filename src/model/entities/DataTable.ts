import { IDataTable, IDataTableData } from "./types/IDataTable";
import { observable, action, computed } from "mobx";
import { IProperty } from "./types/IProperty";
import { getDataView } from "../selectors/DataView/getDataView";
import { IAdditionalRowData } from "./types/IAdditionalRecordData";
import { AdditionalRowData } from "./AdditionalRowData";

export class DataTable implements IDataTable {

  $type_IDataTable: 1 = 1;

  constructor(data: IDataTableData) {
    Object.assign(this, data);
  }

  @observable.shallow allRows: any[][] = [];

  @computed get rows(): any[][] {
    return this.allRows.filter(row => !this.isRowDirtyDeleted(row));
  }
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

  getCellText(row: any[], property: IProperty) {
    const value = this.getCellValue(row, property);
    return this.resolveCellText(property, value);
  }

  resolveCellText(property: IProperty, value: any): any {
    if (property.isLookup) {
      return property.lookup!.getValue(value);
    } else {
      return value;
    }
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

  getNearestRow(row: any[]): any[] | undefined {
    const id = this.getRowId(row);
    let idx = this.getExistingRowIdxById(id);
    if (this.rows.length === 1) return;
    if (idx !== undefined) {
      if (idx === 0) {
        return this.rows[1];
      } else {
        return this.rows[idx - 1];
      }
    } else return;
  }

  getNextExistingRowId(id: string): string | undefined {
    const idx = this.rows.findIndex(
      r => this.getRowId(r) === id
    );
    if (idx > -1) {
      const newRow  = this.rows[idx + 1];
      return newRow ? this.getRowId(newRow) : undefined
    }
  }

  getPrevExistingRowId(id: string): string | undefined {
    const idx = this.rows.findIndex(
      r => this.getRowId(r) === id
    );
    if (idx > 0) {
      const newRow  = this.rows[idx - 1];
      return newRow ? this.getRowId(newRow) : undefined
    }
  }

  getAdditionalRowData(row: any[]) {
    return this.getAdditionalRowDataById(this.getRowId(row));
  }

  getAdditionalRowDataById(id: string) {
    return this.additionalRowData.get(id);
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
    return this.allRows.filter(row => this.isRowDirtyDeleted(row));
  }

  getDirtyNewRows(): any[][] {
    return this.rows.filter(row => this.isRowDirtyNew(row));
  }

  getRowFromDataSourceRow(rowIn: any[]): any[] {
    const result: any[] = [];
    for (let property of this.properties) {
      result.push(rowIn[property.dataSourceIndex]);
    }
    return result;
  }

  @action.bound
  setRecords(rows: any[][]) {
    this.clear();
    this.allRows.push(...rows);
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
  clearUnneededAdditionalRowData() {
    for (let [k, v] of Array.from(this.additionalRowData.entries())) {
      if (
        !v.dirtyDeleted &&
        !v.dirtyNew &&
        v.dirtyFormValues.size === 0 &&
        v.dirtyValues.size === 0
      ) {
        this.additionalRowData.delete(k);
      }
    }
  }

  @action.bound
  deleteAdditionalRowData(row: any[]) {
    this.additionalRowData.delete(this.getRowId(row));
  }

  @action.bound
  deleteRow(row: any[]): void {
    this.deleteAdditionalRowData(row);
    const idx = this.allRows.findIndex(
      r => this.getRowId(r) === this.getRowId(row)
    );
    if (idx > -1) {
      this.allRows.splice(idx, 1);
    }
  }

  @action.bound
  clearRecordDirtyValues(id: string): void {
    const ard = this.getAdditionalRowDataById(id);
    if (ard) {
      ard.dirtyFormValues.clear();
      ard.dirtyValues.clear();
      this.clearUnneededAdditionalRowData();
    }
  }

  @action.bound
  substituteRecord(row: any[]): void {
    const idx = this.allRows.findIndex(
      r => this.getRowId(r) === this.getRowId(row)
    );
    if (idx > -1) {
      this.allRows.splice(idx, 1, row);
    }
  }

  @action.bound
  insertRecord(index: number, row: any[]): void {
    const idx = this.allRows.findIndex(
      r => this.getRowId(r) === this.getRowId(row)
    );
    if (idx > -1) {
      this.allRows.splice(idx, 0, row);
    } else {
      this.allRows.push(row);
    }
  }

  @action.bound
  clear(): void {
    this.allRows.length = 0;
    this.additionalRowData.clear();
  }

  parent?: any;
}
