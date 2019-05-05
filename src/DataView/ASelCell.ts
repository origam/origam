import { IASelCell } from "./types/IASelCell";
import { L, ML } from "../utils/types";
import { IASelRec } from "./types/IASelRec";
import { IASelProp } from "./types/IASelProp";
import { IRecCursor } from "./types/IRecCursor";
import { IPropCursor } from "./types/IPropCursor";
import { IDataTable } from "./types/IDataTable";
import { IPropReorder } from "./types/IPropReorder";
import { IAStartEditing } from "./types/IAStartEditing";
import { IEditing } from "./types/IEditing";
import { action, computed } from "mobx";
import { unpack } from "../utils/objects";


export class ASelCell implements IASelCell {
  constructor(
    public P: {
      aSelRec: L<IASelRec>;
      aSelProp: L<IASelProp>;
      recCursor: ML<IRecCursor>;
      propCursor: ML<IPropCursor>;
      dataTable: ML<IDataTable>;
      propReorder: ML<IPropReorder>;
      aStartEditing: ML<IAStartEditing>;
      editing: ML<IEditing>;
    }
  ) {}

  @action.bound
  doByIdx(rowIdx: number | undefined, colIdx: number | undefined) {
    const isSameCell = rowIdx === this.selRecIdx && colIdx === this.selPropIdx;

    this.P.aSelProp().doByIdx(colIdx);
    this.P.aSelRec().doByIdx(rowIdx);
    if (isSameCell) {
      this.aStartEditing.do();
    }
  }

  @action.bound
  doSelFirst() {
    // debugger
    this.P.aSelProp().doSelFirst();
    this.P.aSelRec().doSelFirst();
  }

  @computed get selPropIdx() {
    return this.propCursor.selId
      ? this.propReorder.getIndexById(this.propCursor.selId)
      : undefined;
  }

  @computed get selRecIdx() {
    return this.recCursor.selId
      ? this.dataTable.getRecordIndexById(this.recCursor.selId)
      : undefined;
  }

  get recCursor() {
    return unpack(this.P.recCursor);
  }

  get propCursor() {
    return unpack(this.P.propCursor);
  }

  get aStartEditing() {
    return unpack(this.P.aStartEditing);
  }

  get aSelProp() {
    return unpack(this.P.aSelProp);
  }

  get aSelRec() {
    return unpack(this.P.aSelRec);
  }

  get dataTable() {
    return unpack(this.P.dataTable);
  }

  get propReorder() {
    return unpack(this.P.propReorder);
  }
}
