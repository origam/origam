import {IProperty} from "model/entities/types/IProperty";

export interface IRowsContainer {
  clear(): void;

  delete(row: any[]): void;

  insert(index: number, row: any[], shouldLockNewRowAtTop?: boolean): Promise<any>;

  set(rows: any[][], rowOffset: number, isFinal: boolean | undefined): Promise<any>;

  appendRecords(rowsIn: any[][]): void;

  substitute(row: any[]): void;

  registerResetListener(listener: () => void): void;

  unlockAddedRowPosition(): void;

  addedRowPositionLocked: boolean;

  rows: any[];

  allRows: any[];

  getFilteredRows(args:{propertyFilterIdToExclude: string}): any[];

  updateSortAndFilter(data?: {retainPreviousSelection?: true}): Promise<any>;

  start(): void;

  stop():void;

  getFirstRow(): any[] | undefined;

  parent?: any;

  getTrueIndexById(id: string): number | undefined;
}
