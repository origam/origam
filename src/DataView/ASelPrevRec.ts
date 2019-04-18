import { action } from "mobx";
import { L, ML } from "../utils/types";
import { IASelPrevRec } from "./types/IASelPrevRec";
import { IRecords } from "./types/IRecords";
import { IRecCursor } from "./types/IRecCursor";
import { IASelRec } from "./types/IASelRec";
import { unpack } from "../utils/objects";

export class ASelPrevRec implements IASelPrevRec {
  constructor(
    public P: {
      records: ML<IRecords>;
      recCursor: ML<IRecCursor>;
      aSelRec: ML<IASelRec>;
    }
  ) {}

  @action.bound do() {
    const { selId } = this.recCursor;
    const id1 = selId && this.records.getIdBeforeId(selId);
    if (id1) {
      this.aSelRec.do(id1);
    }
  }

  get records() {
    return unpack(this.P.records);
  }

  get aSelRec() {
    return unpack(this.P.aSelRec);
  }

  get recCursor() {
    return unpack(this.P.recCursor);
  }
}
