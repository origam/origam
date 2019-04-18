import { L } from "../../utils/types";


interface IASelCell {
  doByIdx(rowIdx: number, colIdx: number): void;
}

export class AOnCellClick {
  constructor(
    public P: {
      aSelCell: L<IASelCell>;
    }
  ) {}

  do(event: any, rowIdx: number, colIdx: number) {
    this.P.aSelCell().doByIdx(rowIdx, colIdx);
  }
}
