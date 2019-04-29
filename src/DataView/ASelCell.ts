import { L } from "../utils/types";
import { action } from "mobx";
import { IASelCell } from "./types/IASelCell";
import { IASelRec } from "./types/IASelRec";
import { IASelProp } from "./types/IASelProp";

export class ASelCell implements IASelCell {
  constructor(public P: {
    aSelRec: L<IASelRec>,
    aSelProp: L<IASelProp>
  }) { }

  @action.bound
  doByIdx(rowIdx: number | undefined, colIdx: number | undefined) {
    this.P.aSelRec().doByIdx(rowIdx);
    this.P.aSelProp().doByIdx(colIdx);
  }

  @action.bound
  doSelFirst() {
    // debugger
    this.P.aSelRec().doSelFirst();
    this.P.aSelProp().doSelFirst();
  }



}
