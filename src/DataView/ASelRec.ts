import { action } from "mobx";
import { L, ML } from "../utils/types";
import { IASelRec } from "./types/IASelRec";
import { IEditing } from "./types/IEditing";
import { IRecCursor } from "./types/IRecCursor";
import { IPropCursor } from "./types/IPropCursor";
import { IAFinishEditing } from "./types/IAFinishEditing";
import { IAStartEditing } from "./types/IAStartEditing";
import { IRecords } from "./types/IRecords";
import { unpack } from "../utils/objects";
import { IAReloadChildren } from "./types/IAReloadChildren";
import { IASelCell } from "./types/IASelCell";

export class ASelRec implements IASelRec {
  constructor(
    public P: {
      aSelCell: ML<IASelCell>;
    }
  ) {}

  @action.bound
  do(id: string | undefined) {
    this.aSelCell.do(id, undefined);
  }

  @action.bound doByIdx(idx: number | undefined) {
    this.aSelCell.doByIdx(idx, undefined);
  }

  @action.bound
  doSelFirst() {
    this.doByIdx(0);
  }

  get aSelCell() {
    return unpack(this.P.aSelCell);
  }
}
