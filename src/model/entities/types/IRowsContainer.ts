export interface IRowsContainer {
  clear(): void;

  delete(row: any[]): void;

  insert(index: number, row: any[]): void;

  set(rows: any[][]): void;

  substitute(row: any[]): void;

  registerResetListener(listener: () => void): void;

  unlockAddedRowPosition(): void;

  maxRowCountSeen: number;

  loadedRowsCount: number;

  rows: any[];

  allRows: any[];
}
