export interface IASelRec {
  do(id: string | undefined): void;
  doByIdx(idx: number | undefined): void;
  doSelFirst(): void;
}