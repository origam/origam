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

import { IDataTable } from "./types/IDataTable";
import { IProperty } from "./types/IProperty";
import { IRowsContainer } from "./types/IRowsContainer";
import { IAdditionalRowData } from "./types/IAdditionalRecordData";
import { IDataSourceField } from "./types/IDataSourceField";
import { computed, observable } from "mobx";
import { getDataSource } from "../selectors/DataSources/getDataSource";

export class TreeDataTable implements IDataTable {
  $type_IDataTable: 1 = 1;
  $type_TreeDataTable: 1 = 1;
  properties: IProperty[] = [];
  rowsContainer: IRowsContainer = null as any;
  rowsAddedSinceSave = 0;
  @observable.shallow rows: any[] = [];

  get allRows(): any[][] {
    return this.rows;
  }

  start() {

  }

  stop() {

  }

  @computed get identifierProperty() {
    return this.properties.find((prop) => prop.id === this.dataSource.identifier);
  }

  get identifierDataIndex() {
    return this.identifierProperty!.dataIndex;
  }

  additionalRowData: Map<string, IAdditionalRowData> = new Map();
  parentIdProperty: string;
  isEmpty: boolean = false;
  idProperty: string;

  constructor(idProperty: string, parentIdProperty: string) {
    this.parentIdProperty = parentIdProperty;
    this.idProperty = idProperty;
  }

  getTrueIndexById(id: string): number | undefined {
    const idx = this.rows.findIndex((row) => this.getRowId(row) === id);
    return idx > -1 ? idx : undefined;
  }

  deleteAdditionalCellData(row: any[], propertyId: string): void {
    throw new Error("Method not implemented.");
  }

  rowRemovedListeners: (() => void)[] = [];

  getRowId(row: any[]): string {
    return row[this.dataSource.getFieldByName(this.idProperty)!.index];
  }

  getCellValue(row: any[], property: IProperty): any {
    throw new Error("Not implemented");
  }

  getOriginalCellValue(row: any[], property: IProperty): any {
    throw new Error("Not implemented");
  }

  getCellValueByDataSourceField(row: any[], dsField: IDataSourceField) {
    return row[dsField.index];
  }

  getCellText(row: any[], property: IProperty): any {
    throw new Error("Not implemented");
  }

  getOriginalCellText(row: any[], property: IProperty): any {
    throw new Error("Not implemented");
  }

  resolveCellText(property: IProperty, value: any): any {
    throw new Error("Not implemented");
  }

  getRowByExistingIdx(idx: number): any[] {
    return this.rows[idx];
  }

  getRowById(id: string): any[] | undefined {
    return this.rows.find((row) => this.getRowId(row) === id);
  }

  getExistingRowIdxById(id: string): number | undefined {
    for (let i = 0; i < this.rows.length; i++) {
      if (this.getRowId(this.rows[i]) === id) {
        return i;
      }
    }
  }

  getPropertyById(id: string): IProperty | undefined {
    return undefined;
  }

  getFirstRow(): any[] | undefined {
    if (this.rows.length > 0) {
      return this.rows[0];
    } else {
      return undefined;
    }
  }

  getNearestRow(row: any[]): any[] | undefined {
    throw new Error("Not implemented");
  }

  getNextExistingRowId(id: string): string | undefined {
    throw new Error("Not implemented");
  }

  getPrevExistingRowId(id: string): string | undefined {
    throw new Error("Not implemented");
  }

  getDirtyValues(row: any[]): Map<string, any> {
    return new Map<string, any>();
  }

  getDirtyValueRows(): any[][] {
    return [];
  }

  getAllValuesOfProp(property: IProperty): Set<any> {
    throw new Error("Not implemented");
  }

  async setRecords(rows: any[][]): Promise<any> {
    this.rows = [...this.sortTreeRows(rows, null)];
  }

  appendRecords(rows: any[][]): void {
    this.rows = [...this.rows, ...this.sortTreeRows(rows, null)];
  }

  private*sortTreeRows(rows: any[], parent: any[] | null): Generator {
    const children = this.getChildren(rows, parent);
    if (parent) {
      yield parent;
    }
    for (let child of children) {
      yield*this.sortTreeRows(rows, child);
    }
  }

  private getChildren(rows: any[], parent: any[] | null) {
    const parentId = parent ? this.getRowId(parent) : null;
    return rows
      .filter((row) => parentId === this.getParentId(row))
      .sort((row1, row2) => this.compareLabels(row1, row2));
  }

  setFormDirtyValue(row: any[], propertyId: string, value: any): void {
    throw new Error("Not implemented");
  }

  setDirtyValue(row: any[], columnId: string, value: any): void {
    throw new Error("Not implemented");
  }

  flushFormToTable(row: any[]): void {
  }

  deleteAdditionalRowData(row: any[]): void {
    throw new Error("Not implemented");
  }

  deleteRow(row: any[]): void {
    throw new Error("Not implemented");
  }

  clear(): void {
    throw new Error("Not implemented");
  }

  clearRecordDirtyValues(rows: any[][]): void {

  }

  substituteRecords(rows: any[][]) {
    for (let row of rows) {
      this.substituteRecord(row);
    }
  }

  substituteRecord(row: any[]): void {
    const idx = this.allRows.findIndex((r) => this.getRowId(r) === this.getRowId(row));
    if (idx > -1) {
      this.allRows.splice(idx, 1, row);
    }
  }

  insertRecord(index: number, row: any[], shouldLockNewRowAtTop?: boolean): Promise<any> {
    throw new Error("Not implemented");
  }

  @computed get dataSource() {
    return getDataSource(this);
  }

  parent?: any;

  getLastRow(): any[] | undefined {
    return undefined;
  }

  getParentId(row: any) {
    const dataSourceField = this.dataSource.getFieldByName(this.parentIdProperty)!;
    return this.getCellValueByDataSourceField(row, dataSourceField);
  }

  getLabel(row: any[]) {
    const dataSourceField = this.dataSource.getFieldByName("Name")!;
    return this.getCellValueByDataSourceField(row, dataSourceField);
  }

  compareLabels(row1: any[], row2: any[]) {
    return this.getLabel(row1).localeCompare(this.getLabel(row2));
  }

  isCellTextResolving(property: IProperty, value: any): boolean {
    return false;
  }

  unlockAddedRowPosition(): void {
  }

  async updateSortAndFilter(data?: { retainPreviousSelection?: true }) {
  }

  addedRowPositionLocked: boolean = false;
}

export const isTreeDataTable = (o: any): o is TreeDataTable => o.$type_TreeDataTable;
