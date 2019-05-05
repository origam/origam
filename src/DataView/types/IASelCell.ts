export interface IASelCell {
  doByIdx(rowIdx: number | undefined, colIdx: number | undefined): void;
  do(rowId: string | undefined, colId: string | undefined): void;
  doSelFirst(): void;
}
