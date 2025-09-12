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

import { action, computed, observable } from "mobx";
import {
  IObjectTouchMover,
  ITouchMoveControlee,
} from "../util/ObjectTouchMover";
import { HashtagRootStore } from "./RootStore";
import { TableCursor } from "./TableCursorStore";

export interface IDataSourceField {
  id: string;
  dataIndex: number;
}

export interface IDataSource {
  id: string;
  fields: IDataSourceField[];
  fieldById: Map<string, IDataSourceField>;

  clearFields(): void;
  getFieldById(id: string): IDataSourceField | undefined;
}

export class DataSourceField implements IDataSourceField {
  constructor(public id: string, public dataIndex: number) {}
}

export class DataSource implements IDataSource {
  constructor(public id: string) {}

  @observable fields: IDataSourceField[] = [];

  @computed get fieldById() {
    return new Map(this.fields.map((item) => [item.id, item]));
  }

  @action.bound clearFields() {
    this.fields.length = 0;
  }

  getFieldById(id: string) {
    return this.fieldById.get(id);
  }
}

export interface IColumn {
  id: string;
  width: number;
  name: string;
  dataIndex: number | undefined;
  renderer: string;
  formatterPattern: string;
  touchMover?: IObjectTouchMover;
}

export interface IDataTable {
  id: string;
  dataSource: IDataSource;
  columns: IColumn[];
  columnById: Map<string, IColumn>;

  tableCursor: TableCursor;

  rows: any[][];
  identifierIndex: number;
  selectedRowIds: Set<any>;
  selectedRowCount: number;
  rowCount: number;
  columnCount: number;
  getColumnByDataCellIndex(index: number): IColumn;
  getRowByDataCellIndex(index: number): any[];
  getDataSourceFieldById(id: string): IDataSourceField | undefined;

  getRowById(rowId: string): any[][] | undefined;
  selectedRow: any[] | undefined;

  getRowId(row: any[]): any;

  getSelectionStateByRowId(rowId: string): boolean;

  setRows(rows: any[][]): void;
  appendRows(rows: any[][]): void;
  clearSelectedRows(): void;
  clearData(): void;

  clearColumns(): void;

  handleSelectionChange(event: any, rowId: string, state: boolean): void;
  handleDataCellClick(event: any, rowIndex: number, columnIndex: number): void;
  handleSelectionCellClick(event: any, rowIndex: number): void;
}

export interface IColumnOwner {
  getDataSourceFieldById(id: string): IDataSourceField | undefined;
}

export class Column2TouchMoveControlee implements ITouchMoveControlee {
  constructor(public column: IColumn) {}

  getInitialCoords(): { x: number; y: number } {
    return {
      x: this.column.width,
      y: 0,
    };
  }

  setCoords(coords: { x: number; y: number; dx: number; dy: number }): void {
    this.column.width = Math.max(30, coords.x);
  }
}

export class Column implements IColumn {
  constructor(
    public owner: IColumnOwner,
    public id: string,
    name: string,
    public renderer: string,
    public formatterPattern: string
  ) {
    this.name = name;
  }

  @observable name: string;
  @observable width: number = 150;

  touchMover?: IObjectTouchMover;

  get dataIndex() {
    return this.owner.getDataSourceFieldById(this.id)?.dataIndex;
  }
}

export class DataTable implements IDataTable, IColumnOwner {
  constructor(
    public tableCursor: TableCursor,
    public id: string,
    public identifierIndex: number
  ) {}

  @observable selectedRowIds = new Set<any>();
  dataSource: IDataSource = null!;
  @observable columns: IColumn[] = [];
  @observable rows: any[][] = [];

  @computed get columnById() {
    return new Map(this.columns.map((item) => [item.id, item]));
  }

  get rowCount() {
    return this.rows.length;
  }

  get columnCount() {
    return this.columns.length;
  }

  get selectedRowCount() {
    return this.selectedRowIds.size;
  }

  @computed get selectedRow() {
    return this.tableCursor.selectedRowId
      ? this.getRowById(this.tableCursor.selectedRowId)
      : undefined;
  }

  getColumnByDataCellIndex(index: number) {
    return this.columns[index];
  }

  getRowByDataCellIndex(index: number) {
    return this.rows[index];
  }

  getRowById(rowId: string): any[][] | undefined {
    return this.rows.find((row) => this.getRowId(row) === rowId);
  }

  getDataSourceFieldById(id: string) {
    return this.dataSource.getFieldById(id);
  }

  getRowId(row: any[]) {
    return row[this.identifierIndex];
  }

  getSelectionStateByRowId(rowId: string) {
    return this.selectedRowIds.has(rowId);
  }

  @action.bound setRows(rows: any[][]) {
    this.rows = rows;
  }

  @action.bound appendRows(rows: any[][]) {
    for (let row of rows) this.rows.push(row);
  }

  @action.bound clearData() {
    this.rows.length = 0;
  }

  @action.bound clearColumns() {
    this.columns.length = 0;
  }

  clearSelectedRows(): void {
    this.selectedRowIds.clear();
  }

  handleSelectionChange(event: any, rowId: string, state: boolean): void {
    if (state) {
      this.selectedRowIds.add(rowId);
    } else {
      this.selectedRowIds.delete(rowId);
    }
  }

  @action.bound handleDataCellClick(
    event: any,
    rowIndex: number,
    columnIndex: number
  ): void {
    const row = this.getRowByDataCellIndex(rowIndex);
    const rowId = row && this.getRowId(row);
    if (rowId) this.tableCursor.setSelectedRowId(rowId);
  }

  @action.bound handleSelectionCellClick(event: any, rowIndex: number): void {
    const row = this.getRowByDataCellIndex(rowIndex);
    const rowId = row && this.getRowId(row);
    if (rowId) this.tableCursor.setSelectedRowId(rowId);
  }
}

export class DataTableStore {
  constructor(private root: HashtagRootStore) {}

  @observable dataTables: IDataTable[] = [];

  @computed get dataTableById(): Map<string, IDataTable> {
    return new Map(this.dataTables.map((item) => [item.id, item]));
  }

  getDataTable(entityId: string) {
    return this.dataTableById.get(entityId);
  }
}
