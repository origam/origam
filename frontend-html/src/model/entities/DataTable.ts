/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { IDataTable, IDataTableData } from "./types/IDataTable";
import { action, computed, observable } from "mobx";
import { IProperty } from "./types/IProperty";
import { getDataView } from "../selectors/DataView/getDataView";
import { IAdditionalRowData } from "./types/IAdditionalRecordData";
import { AdditionalRowData } from "./AdditionalRowData";
import { IDataSourceField } from "./types/IDataSourceField";
import { getRowContainer } from "../selectors/getRowContainer";
import { IRowsContainer } from "./types/IRowsContainer";
import { formatNumber } from "./NumberFormating";
import { getDataSource } from "model/selectors/DataSources/getDataSource";
import { getDataSourceFieldByIndex } from "model/selectors/DataSources/getDataSourceFieldByIndex";
import { isArray } from "lodash";

export class DataTable implements IDataTable {
  $type_IDataTable: 1 = 1;
  rowsContainer: IRowsContainer = null as any;
  @observable
  isEmpty: boolean = false;
  @observable
  rowsAddedSinceSave = 0;

  constructor(data: IDataTableData) {
    Object.assign(this, data);
    this.rowsContainer = getRowContainer(
      data.formScreenLifecycle,
      data.dataViewAttributes,
      data.orderingConfiguration,
      data.filterConfiguration,
      (row: any[]) => this.getRowId(row),
      this
    );
  }

  rowRemovedListeners: (() => void)[] = [];

  notifyRowRemovedListeners() {
    this.rowRemovedListeners.forEach((listener) => listener());
  }

  async start() {
    await this.rowsContainer.start();
  }

  stop() {
    this.rowsContainer.stop();
  }

  get allRows() {
    return this.rowsContainer.allRows;
  }

  @computed get rows(): any[][] {
    return this.rowsContainer.rows;
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

  get identifierDataIndex() {
    return this.identifierProperty!.dataIndex;
  }

  getRowId(row: any[]): string {
    return row[this.identifierDataIndex];
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
    return this.getOriginalCellValue(row, property);
  }

  getOriginalCellValue(row: any[], property: IProperty) {
    return row[property.dataIndex];
  }

  async updateSortAndFilter(data?: { retainPreviousSelection?: true }) {
    return this.rowsContainer.updateSortAndFilter(data);
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

  // Returns all values from currently loaded rows (in case the table is infinitely scrolled)
  getAllValuesOfProp(property: IProperty): Set<any> {
    const values = this.rowsContainer
      .getFilteredRows({propertyFilterIdToExclude: property.id})
      .map((row) => this.getCellValue(row, property)).filter((row) => row)
    if (values.some(value => isArray(value))) {
      return new Set(values.flatMap(array => array));
    }
    return new Set(values);
  }

  getCellText(row: any[], property: IProperty) {
    const value = this.getCellValue(row, property);
    return this.resolveCellText(property, value);
  }

  getOriginalCellText(row: any[], property: IProperty) {
    const value = this.getOriginalCellValue(row, property);
    return this.resolveCellText(property, value);
  }

  resolveCellText(property: IProperty, value: any): any {
    if (value === null || value === undefined || (Array.isArray(value) && value.length === 0))
      return "";
    if (property.isLookup && property.lookupEngine) {
      const {lookupEngine} = property;
      if (property.column === "TagInput") {
        if (!Array.isArray(value)) value = [value];
        const textArray = value.map((valueItem: any) =>
          lookupEngine.lookupResolver.resolveValue(`${valueItem}`)
        );
        return textArray || [];
      } else {
        if (Array.isArray(value)) {
          return value
            .map((item) => lookupEngine.lookupResolver.resolveValue(`${item}`))
            .join(", ");
        }
        return lookupEngine.lookupResolver.resolveValue(`${value}`);
      }
    }
    if (property.column === "Number") {
      return formatNumber(property.customNumericFormat, property.entity, value);
    }
    return value;
  }

  getRowByExistingIdx(idx: number): any[] {
    return this.rows[idx];
  }

  getRowById(id: string): any[] | undefined {
    return this.rowsContainer.getRowById(id);
  }

  getTrueIndexById(id: string) {
    return this.rowsContainer.getTrueIndexById(id);
  }

  getExistingRowIdxById(id: string) {
    const idx = this.rows.findIndex((row) => this.getRowId(row) === id);
    return idx > -1 ? idx : undefined;
  }

  isCellTextResolving(property: IProperty, value: any): boolean {
    if (value === null || value === undefined) return false;
    if (property.isLookup && property.lookupEngine) {
      const {lookupEngine} = property;
      if (property.column === "TagInput") {
        return value.some((valueItem: any) =>
          lookupEngine.lookupResolver.isEmptyAndLoading(`${valueItem}`)
        );
      } else {
        return lookupEngine.lookupResolver.isEmptyAndLoading(`${value}`);
      }
    } else {
      return false;
    }
  }

  getPropertyById(id: string) {
    return this.properties.find((prop) => prop.id === id);
  }

  getFirstRow(): any[] | undefined {
    return this.rowsContainer.getFirstRow();
  }

  getLastRow(): any[] | undefined {
    if (this.rows.length === 0) {
      return undefined;
    }
    return this.rows[this.rows.length - 1];
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

  @action.bound
  appendRecords(rows: any[][]): void {
    this.rowsContainer.appendRecords(rows);
  }

  @action.bound
  async setRecords(rows: any[][]) {
    this.clear();
    await this.rowsContainer.set(rows, 0, undefined);
    if (rows.length === 0) {
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
        v.dirtyFormValues.size === 0 &&
        v.dirtyValues.size === 0
      ) {
        this.additionalRowData.delete(k);
      }
    }
  }

  @action.bound
  deleteAdditionalCellData(row: any[], propertyId: string) {
    const additionalData = this.additionalRowData.get(this.getRowId(row));
    if (!additionalData) {
      return;
    }
    additionalData.dirtyFormValues.delete(propertyId);
    additionalData.dirtyValues.delete(propertyId);
  }

  @action.bound
  deleteAdditionalRowData(row: any[]) {
    this.additionalRowData.delete(this.getRowId(row));
  }

  @action.bound
  deleteRow(row: any[]): void {
    this.deleteAdditionalRowData(row);
    this.rowsContainer.delete(row);
    this.notifyRowRemovedListeners();
    this.rowsAddedSinceSave--;
  }

  @action.bound
  clearRecordDirtyValues(rows: any[]): void {
    for (let row of rows) {
      const rowId = this.getRowId(row);
      const rowData = this.getAdditionalRowDataById(rowId);
      if (rowData) {
        const oldRow = this.getRowById(rowId)!;
        for (let i = 0; i < row.length; i++) {
          const dataSourceField = getDataSourceFieldByIndex(this, i)!;
          if (row[i] !== oldRow[i]) {
            rowData.dirtyFormValues.delete(dataSourceField.name);
            rowData.dirtyValues.delete(dataSourceField.name);
          }
          if (rowData.dirtyFormValues.get(dataSourceField.name) === row[i]) {
            rowData.dirtyFormValues.delete(dataSourceField.name);
          }
          if (rowData.dirtyValues.get(dataSourceField.name) === row[i]) {
            rowData.dirtyValues.delete(dataSourceField.name);
          }
        }
        this.clearUnneededAdditionalRowData();
      }
    }
  }

  @action.bound
  substituteRecord(row: any[]): void {
    this.rowsContainer.substituteRows([row]);
  }

  @action.bound
  substituteRecords(rows: any[][]): void {
    this.rowsContainer.substituteRows(rows);
  }

  @action.bound
  async insertRecord(index: number, row: any[], shouldLockNewRowAtTop?: boolean): Promise<any> {
    await this.rowsContainer.insert(index, row, shouldLockNewRowAtTop);
    this.rowsAddedSinceSave++;
  }

  @action.bound
  clear(): void {
    this.rowsContainer.clear();
    this.additionalRowData.clear();
    this.notifyRowRemovedListeners();
  }

  unlockAddedRowPosition(): void {
    this.rowsContainer.unlockAddedRowPosition();
  }

  get addedRowPositionLocked(): boolean {
    return this.rowsContainer.addedRowPositionLocked;
  }

  parent?: any;
}
