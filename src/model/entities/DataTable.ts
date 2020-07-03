import {IDataTable, IDataTableData} from "./types/IDataTable";
import {action, computed, observable} from "mobx";
import {IProperty} from "./types/IProperty";
import {getDataView} from "../selectors/DataView/getDataView";
import {IAdditionalRowData} from "./types/IAdditionalRecordData";
import {AdditionalRowData} from "./AdditionalRowData";
import {getDataSource} from "../selectors/DataSources/getDataSource";
import {IDataSourceField} from "./types/IDataSourceField";
import {getGrouper} from "model/selectors/DataView/getGrouper";
import {IGroupTreeNode} from "gui/Components/ScreenElements/Table/TableRendering/types";
import {getRowContainer} from "../selectors/getRowContainer";
import {IRowsContainer} from "./types/IRowsContainer";
import {formatNumber} from "./NumberFormating";

export class DataTable implements IDataTable {
  $type_IDataTable: 1 = 1;
  rowsContainer: IRowsContainer = null as any;
  @observable
  isEmpty: boolean = false;

  constructor(data: IDataTableData) {
    Object.assign(this, data);
    this.rowsContainer = getRowContainer(
      data.formScreenLifecycle,
      data.dataViewAttributes,
      data.orderingConfiguration,
      data.filterConfiguration,
      (row: any[]) => this.getRowId(row))
  }

  get allRows(){
    return this.rowsContainer.rows;
  }

  @computed get groups(): IGroupTreeNode[] {
    return getGrouper(this).topLevelGroups;
  }

  @computed get rows(): any[][] {
    return this.rowsContainer.rows.filter(row => !this.isRowDirtyDeleted(row));
  }
  @observable additionalRowData: Map<string, IAdditionalRowData> = new Map();

  @computed
  get properties(): IProperty[] {
    return getDataView(this).properties;
  }

  @computed get dataSource() {
    return getDataSource(this);
  }

  @computed get identifierProperty() {
    return this.properties.find((prop) => prop.id === this.dataSource.identifier);
  }

  @computed get maxRowCountSeen() {
    if(this.groups.length > 0){
      return this.groups
        .map(group => group.rowCount)
        .reduce((x, y) => x + y);
    }else{
      return this.rowsContainer.maxRowCountSeen;
    }
  }

  getRowId(row: any[]): string {
    return row[this.identifierProperty!.dataIndex];
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

  getCellValueByDataSourceField(row: any[], dsField: IDataSourceField) {
    if (this.additionalRowData.has(this.getRowId(row))) {
      const ard = this.additionalRowData.get(this.getRowId(row))!;
      if (ard.dirtyFormValues.has(dsField.name)) {
        return ard.dirtyFormValues.get(dsField.name);
      }
      if (ard.dirtyValues.has(dsField.name)) {
        return ard.dirtyValues.get(dsField.name);
      }
    }
    return row[dsField.index];
  }

  getAllValuesOfProp(property: IProperty): any[] {
    return this.rowsContainer.rows.map(row => this.getCellValue(row, property));
  }

  getCellText(row: any[], property: IProperty) {
    const value = this.getCellValue(row, property);
    return this.resolveCellText(property, value);
  }

  resolveCellText(property: IProperty, value: any): any {
    if (value === null || value === undefined) return "";
    if (property.isLookup) {
      if (property.column === "TagInput") {
        const textArray = value.map((valueItem: any) => property.lookup!.getValue(`${valueItem}`));
        return (textArray || []).join(", ");
      } else {
        return property.lookup!.getValue(`${value}`);
      }
    }
    if(property.column === "Number"){
      return formatNumber(property.customNumericFormat, property.entity,  value);
    }
    return value;
  }

  getRowByExistingIdx(idx: number): any[] {
    // TODO: Change to respect dirty deleted rows.
    return this.rows[idx];
  }

  getRowById(id: string): any[] | undefined {
    return this.rowsContainer.rows.find(row => this.getRowId(row) === id);
  }

  getExistingRowIdxById(id: string) {
    const idx = this.rows.findIndex((row) => this.getRowId(row) === id);
    return idx > -1 ? idx : undefined;
  }

  getPropertyById(id: string) {
    return this.properties.find((prop) => prop.id === id);
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
    const idx = this.rows.findIndex((r) => this.getRowId(r) === id);
    if (idx > -1) {
      const newRow = this.rows[idx + 1];
      return newRow ? this.getRowId(newRow) : undefined;
    }
  }

  getPrevExistingRowId(id: string): string | undefined {
    const idx = this.rows.findIndex((r) => this.getRowId(r) === id);
    if (idx > 0) {
      const newRow = this.rows[idx - 1];
      return newRow ? this.getRowId(newRow) : undefined;
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
    return this.rows.filter((row) => this.hasRowDirtyValues(row));
  }

  getDirtyDeletedRows(): any[][] {
    return this.rowsContainer.rows.filter(row => this.isRowDirtyDeleted(row));
  }

  getDirtyNewRows(): any[][] {
    return this.rows.filter((row) => this.isRowDirtyNew(row));
  }

  @action.bound
  setRecords(rows: any[][]) {
    this.clear();
    this.rowsContainer.set(rows);
    if(rows.length === 0){
      this.isEmpty = true;
    }
  }
  @action.bound
  setFormDirtyValue(row: any[], propertyId: string, value: any) {
    this.createAdditionalData(row);
    this.additionalRowData.get(this.getRowId(row))!.dirtyFormValues.set(propertyId, value);
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

  @action.bound setDirtyValue(row: any[], columnId: string, value: any) {
    this.createAdditionalData(row);
    const ard = this.getAdditionalRowData(row);
    if (ard) {
      ard.dirtyValues.set(columnId, value);
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
    if (!this.additionalRowData.has(this.getRowId(row))) {
      const ard = new AdditionalRowData();
      ard.parent = this;
      this.additionalRowData.set(this.getRowId(row), ard);
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
    this.rowsContainer.delete(row);
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
   this.rowsContainer.substitute(row);
  }

  @action.bound
  insertRecord(index: number, row: any[]): void {
    this.rowsContainer.insert(index, row);
  }

  @action.bound
  clear(): void {
    this.rowsContainer.clear();
    this.additionalRowData.clear();
  }

  parent?: any;
}
