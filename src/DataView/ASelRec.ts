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

export class ASelRec implements IASelRec {
  constructor(
    public P: {
      editing: ML<IEditing>;
      recCursor: ML<IRecCursor>;
      propCursor: ML<IPropCursor>;
      aFinishEditing: ML<IAFinishEditing>;
      aStartEditing: ML<IAStartEditing>;
      records: ML<IRecords>;
    }
  ) {}

  @action.bound
  do(id: string | undefined) {
    // --------------------------------------------------------
    const isEditing = this.editing.isEditing;
    const isRowChange = id !== this.recCursor.selId;

    if (isEditing && isRowChange) {
      this.aFinishEditing.do();
    }
    if (id) {
      this.recCursor.setSelId(id);
    }
    if (
      isEditing &&
      isRowChange &&
      this.recCursor.isSelected &&
      this.propCursor.isSelected
    ) {
      this.aStartEditing.do();
    }
  }

  @action.bound doByIdx(idx: number | undefined) {
    const id = idx !== undefined ? this.records.getIdByIndex(idx) : undefined;
    this.do(id);
  }

  @action.bound
  doSelFirst() {
    // TODO
    this.doByIdx(0);
  }

  get editing() {
    return unpack(this.P.editing);
  }

  get recCursor() {
    return unpack(this.P.recCursor);
  }

  get propCursor() {
    return unpack(this.P.propCursor);
  }

  get aFinishEditing() {
    return unpack(this.P.aFinishEditing);
  }

  get aStartEditing() {
    return unpack(this.P.aStartEditing);
  }

  get records() {
    return unpack(this.P.records);
  }
}
