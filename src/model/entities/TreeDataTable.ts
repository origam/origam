import { IDataTable } from "./types/IDataTable";
import { IProperty } from "./types/IProperty";
import { IRowsContainer } from "./types/IRowsContainer";
import { IAdditionalRowData } from "./types/IAdditionalRecordData";
import { IGroupTreeNode } from "../../gui/Components/ScreenElements/Table/TableRendering/types";
import { IDataSourceField } from "./types/IDataSourceField";
import { computed } from "mobx";
import { getDataSource } from "../selectors/DataSources/getDataSource";

export class TreeDataTable implements IDataTable {
  $type_IDataTable: 1 = 1;
  $type_TreeDataTable: 1 = 1;
  properties: IProperty[] = [];
  rowsContainer: IRowsContainer = null as any;
  rows: any[] = [];

  get allRows(): any[][] {
    return this.rows;
  }

  additionalRowData: Map<string, IAdditionalRowData> = new Map();
  maxRowCountSeen: number = 0;
  groups: IGroupTreeNode[] = [];
  parentIdProperty: string;
  isEmpty: boolean = false;
  idProperty: string;

  constructor(idProperty: string, parentIdProperty: string) {
    this.parentIdProperty = parentIdProperty;
    this.idProperty = idProperty;
  }

  getRowId(row: any[]): string {
    return row[this.dataSource.getFieldByName(this.idProperty)!.index];
  }

  getCellValue(row: any[], property: IProperty): any {
    throw new Error("Not implemented");
  }

  getCellValueByDataSourceField(row: any[], dsField: IDataSourceField) {
    return row[dsField.index];
  }

  getCellText(row: any[], property: IProperty): any {
    throw new Error("Not implemented");
  }

  resolveCellText(property: IProperty, value: any): any {
    throw new Error("Not implemented");
  }

  getRowByExistingIdx(idx: number): any[] {
    return this.rows[idx];
  }

  getRowById(id: string): any[] | undefined {
    throw new Error("Not implemented");
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
    throw new Error("Not implemented");
  }

  getDirtyValueRows(): any[][] {
    throw new Error("Not implemented");
  }

  getDirtyDeletedRows(): any[][] {
    throw new Error("Not implemented");
  }

  getDirtyNewRows(): any[][] {
    throw new Error("Not implemented");
  }

  getAllValuesOfProp(property: IProperty): any[] {
    throw new Error("Not implemented");
  }

  setRecords(rows: any[][]): void {
    this.rows = [...this.sortTreeRows(rows, null)];

    // const ble = [
    //   [
    //     "B",
    //     null,
    //     "2",
    //     false,
    //     "a13f7a11-086b-4976-b912-a374a6cc0edb",
    //     null,
    //     "2",
    //     "2019-11-07T13:17:56.29",
    //     null,
    //     false,
    //     null,
    //   ],
    //
    //   [
    //     "A",
    //     null,
    //     "1",
    //     false,
    //     "91599670-d428-433d-ae75-1664d2049647",
    //     null,
    //     "1",
    //     "2020-02-21T10:42:17.977",
    //     null,
    //     false,
    //     null,
    //   ],
    //
    //   [
    //     "A1",
    //     "1",
    //     "3",
    //     false,
    //     "a13f7a11-086b-4976-b912-a374a6cc0edb",
    //     null,
    //     "3",
    //     "2019-11-07T13:19:16.027",
    //     null,
    //     false,
    //     null,
    //   ],
    //
    //   [
    //     "B1",
    //     "2",
    //     "4",
    //     false,
    //     "a13f7a11-086b-4976-b912-a374a6cc0edb",
    //     null,
    //     "4",
    //     "2019-11-07T13:19:16.027",
    //     null,
    //     false,
    //     null,
    //   ],
    //
    //   [
    //     "A111",
    //     "3",
    //     "5",
    //     false,
    //     "a13f7a11-086b-4976-b912-a374a6cc0edb",
    //     null,
    //     "5",
    //     "2019-11-07T13:19:16.027",
    //     null,
    //     false,
    //     null,
    //   ],
    // ];
    //
    // this.rows = [...this.sortTreeRows(ble, null)];
  }

  private *sortTreeRows(rows: any[], parent: any[] | null): Generator {
    const children = this.getChildren(rows, parent);
    if (parent) {
      yield parent;
    }
    for (let child of children) {
      yield* this.sortTreeRows(rows, child);
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
    throw new Error("Not implemented");
  }

  setDirtyDeleted(row: any[]): void {
    throw new Error("Not implemented");
  }

  setDirtyNew(row: any[]): void {
    throw new Error("Not implemented");
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

  clearRecordDirtyValues(id: string): void {
    throw new Error("Not implemented");
  }

  substituteRecord(row: any[]): void {
    throw new Error("Not implemented");
  }

  insertRecord(index: number, row: any[]): void {
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
}

export const isTreeDataTable = (o: any): o is TreeDataTable => o.$type_TreeDataTable;
