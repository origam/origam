export interface IRowsContainer {
  clear(): void;

  delete(row: any[]): void;

  insert(index: number, row: any[]): void;

  set(rows: any[][]): void;

  substitute(row: any[]): void;

  registerResetListener(listener: () => void): void;

  unlockAddedRowPosition(): void;

  addedRowPositionLocked: boolean;

  maxRowCountSeen: number;

  loadedRowsCount: number;

  rows: any[];

  allRows: any[];

  updateSortAndFilter(): void;

  start(): void;

  stop():void;

  getFirstRow(): any[] | undefined;

  parent?: any;
}
