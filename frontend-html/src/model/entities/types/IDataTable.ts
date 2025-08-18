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

import { IProperty } from "./IProperty";
import { IAdditionalRowData } from "./IAdditionalRecordData";
import { IDataSourceField } from "./IDataSourceField";
import { IFormScreenLifecycle02 } from "./IFormScreenLifecycle";
import { IOrderingConfiguration } from "./IOrderingConfiguration";
import { IFilterConfiguration } from "./IFilterConfiguration";
import { IRowsContainer } from "./IRowsContainer";

export interface IDataTableData {
  formScreenLifecycle: IFormScreenLifecycle02;
  dataViewAttributes: any;
  orderingConfiguration: IOrderingConfiguration;
  filterConfiguration: IFilterConfiguration;
}

export interface IDataTable {
  start(): void;

  stop(): void;

  $type_IDataTable: 1;
  properties: IProperty[];
  rows: any[][];
  allRows: any[][];
  additionalRowData: Map<string, IAdditionalRowData>;
  rowsContainer: IRowsContainer;
  isEmpty: boolean;
  rowRemovedListeners: (() => void)[];
  identifierDataIndex: number;
  rowsAddedSinceSave: number;

  getRowId(row: any[]): string;

  getCellValue(row: any[], property: IProperty): any;

  getOriginalCellValue(row: any[], property: IProperty): any;

  updateSortAndFilter(data?: { retainPreviousSelection?: boolean }): Promise<any>;

  getCellValueByDataSourceField(row: any[], dsField: IDataSourceField): any;

  getCellText(row: any[], property: IProperty): any;

  getOriginalCellText(row: any[], property: IProperty): any;

  resolveCellText(property: IProperty, value: any): any;

  isCellTextResolving(property: IProperty, value: any): boolean;

  getRowByExistingIdx(idx: number): any[];

  getRowById(id: string): any[] | undefined;

  getTrueIndexById(id: string): number | undefined;

  getExistingRowIdxById(id: string): number | undefined;

  getPropertyById(id: string): IProperty | undefined;

  getFirstRow(): any[] | undefined;

  getNearestRow(row: any[]): any[] | undefined;

  getNextExistingRowId(id: string): string | undefined;

  getPrevExistingRowId(id: string): string | undefined;

  getDirtyValues(row: any[]): Map<string, any>;

  getDirtyValueRows(): any[][];

  getAllValuesOfProp(property: IProperty): Set<any>;

  /*setFilteringFn(fn: ((dataTable: IDataTable) => (row: any[]) => boolean)
  | undefined): void;*/

  setRecords(rows: any[][]): Promise<any>;

  appendRecords(rows: any[][]): void;

  setFormDirtyValue(row: any[], propertyId: string, value: any): void;

  setDirtyValue(row: any[], columnId: string, value: any): void;

  flushFormToTable(row: any[]): void;

  deleteAdditionalCellData(row: any[], propertyId: string): void;

  deleteAdditionalRowData(row: any[]): void;

  deleteRow(row: any[]): void;

  clear(): void;

  clearRecordDirtyValues(rows: any[]): void;

  substituteRecords(rows: any[][]): void;

  substituteRecord(row: any[]): void;

  insertRecord(index: number, row: any[], shouldLockNewRowAtTop?: boolean): Promise<any>;

  getLastRow(): any[] | undefined;

  unlockAddedRowPosition(): void;

  addedRowPositionLocked: boolean;
  parent?: any;
}

export const isIDataTable = (o: any): o is IDataTable => o.$type.IDataTable;
