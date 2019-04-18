export interface IASelCell {
  doByIdx(rowIdx: number | undefined, colIdx: number | undefined): void;
  doSelFirst(): void;
}