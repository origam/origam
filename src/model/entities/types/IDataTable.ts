import { IProperty } from "./IProperty";
import { IAdditionalRowData } from "./IAdditionalRecordData";
import { IDataSourceField } from "./IDataSourceField";
import { IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";
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
  loadedRowsCount: number;
  allRows: any[][];
  additionalRowData: Map<string, IAdditionalRowData>;
  maxRowCountSeen: number;
  rowsContainer: IRowsContainer;
  isEmpty: boolean;
  rowRemovedListeners: (() => void)[];
  identifierDataIndex: number;

  getRowId(row: any[]): string;
  getCellValue(row: any[], property: IProperty): any;
  getOriginalCellValue(row: any[], property: IProperty): any;
  updateSortAndFilter(): void;
  getCellValueByDataSourceField(row: any[], dsField: IDataSourceField): any;
  getCellText(row: any[], property: IProperty): any;
  getOriginalCellText(row: any[], property: IProperty): any;
  resolveCellText(property: IProperty, value: any): any;
  isCellTextResolving(property: IProperty, value: any): boolean;
  getRowByExistingIdx(idx: number): any[];
  getRowById(id: string): any[] | undefined;
  getExistingRowIdxById(id: string): number | undefined;
  getPropertyById(id: string): IProperty | undefined;
  getFirstRow(): any[] | undefined;
  getNearestRow(row: any[]): any[] | undefined;
  getNextExistingRowId(id: string): string | undefined;
  getPrevExistingRowId(id: string): string | undefined;

  getDirtyValues(row: any[]): Map<string, any>;
  getDirtyValueRows(): any[][];
  getDirtyDeletedRows(): any[][];
  getDirtyNewRows(): any[][];
  getAllValuesOfProp(property: IProperty): Set<any>;

  /*setFilteringFn(fn: ((dataTable: IDataTable) => (row: any[]) => boolean)
  | undefined): void;*/

  setRecords(rows: any[][]): void;
  setFormDirtyValue(row: any[], propertyId: string, value: any): void;
  setDirtyValue(row: any[], columnId: string, value: any): void;
  flushFormToTable(row: any[]): void;
  setDirtyDeleted(row: any[]): void;
  setDirtyNew(row: any[]): void;
  deleteAdditionalCellData(row: any[], propertyId: string): void;
  deleteAdditionalRowData(row: any[]): void;
  deleteRow(row: any[]): void;
  clear(): void;
  clearRecordDirtyValues(id: string, newRow: any[]): void;
  substituteRecord(row: any[]): void;
  insertRecord(index: number, row: any[]): Promise<any>;
  getLastRow(): any[] | undefined;
  unlockAddedRowPosition(): void;
  addedRowPositionLocked: boolean;
  parent?: any;
}

export const isIDataTable = (o: any): o is IDataTable => o.$type.IDataTable;
