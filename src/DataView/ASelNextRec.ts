import { action } from "mobx";
import { L, ML } from "../utils/types";
import { IASelNextRec } from "./types/IASelNextRec";
import { IRecords } from "./types/IRecords";
import { IRecCursor } from "./types/IRecCursor";
import { IASelRec } from "./types/IASelRec";
import { unpack } from "../utils/objects";

export class ASelNextRec implements IASelNextRec {
  constructor(
    public P: {
      records: ML<IRecords>;
      recCursor: ML<IRecCursor>;
      aSelRec: ML<IASelRec>;
    }
  ) {}

  @action.bound do() {
    const {selId} = this.recCursor;
    const id1 = selId && this.records.getIdAfterId(selId);
    if (id1) {
      this.aSelRec.do(id1);
    }
  }

  get records() {
    return unpack(this.P.records);
  }

  get recCursor() {
    return unpack(this.P.recCursor);
  }

  get aSelRec() {
    return unpack(this.P.aSelRec);
  }
}
