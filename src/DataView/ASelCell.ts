import { action } from "mobx";
import { unpack } from "../utils/objects";
import { ML } from "../utils/types";
import { IAFinishEditing } from "./types/IAFinishEditing";
import { IASelCell } from "./types/IASelCell";
import { IAStartEditing } from "./types/IAStartEditing";
import { IDataTable } from "./types/IDataTable";
import { IEditing } from "./types/IEditing";
import { IForm } from "./types/IForm";
import { IPropCursor } from "./types/IPropCursor";
import { IPropReorder } from "./types/IPropReorder";
import { IRecCursor } from "./types/IRecCursor";

export class ASelCell implements IASelCell {
  constructor(
    public P: {
      recCursor: ML<IRecCursor>;
      propCursor: ML<IPropCursor>;
      dataTable: ML<IDataTable>;
      propReorder: ML<IPropReorder>;
      editing: ML<IEditing>;
      aStartEditing: ML<IAStartEditing>;
      aFinishEditing: ML<IAFinishEditing>;
      form: ML<IForm>;
      dispatch(action: any): void;
      listen(cb: (action: any) => void): void;
      // aOnChange:
    }
  ) {}

  @action.bound
  doByIdx(rowIdx: number | undefined, colIdx: number | undefined) {
    // debugger
    const rowId =
      rowIdx !== undefined
        ? this.dataTable.getRecordIdByIndex(rowIdx)
        : undefined;
    const colId =
      colIdx !== undefined ? this.propReorder.getIdByIndex(colIdx) : undefined;
    this.do(rowId, colId);
  }

  @action.bound do(rowId: string | undefined, colId: string | undefined) {
    if (rowId) {
      this.recCursor.setSelId(rowId);
    }
    if (colId) {
      this.propCursor.setSelId(colId);
    }
  }

  // TODO: Remove this method.
  /*
  @action.bound doOld(rowId: string | undefined, colId: string | undefined) {
    const isSessioned = false;
    const sel0 = {
      rowId: this.recCursor.selId,
      colId: this.propCursor.selId
    };
    const sel1 = {
      rowId: rowId || sel0.rowId,
      colId: colId || sel0.colId
    };
    const willSelect = !!(sel1.rowId && sel1.colId);
    const isRowChange = sel0.rowId !== sel1.rowId;
    const isColChange = sel0.colId !== sel1.colId;
    const isFreshSel = !(sel0.rowId !== undefined && sel0.colId !== undefined);
    const isFinishEditing =
      this.editing.isEditing && (isRowChange || (isColChange && !isSessioned));
    const isStartEditing =
      willSelect &&
      ((!this.editing.isEditing &&
        !isFreshSel &&
        !isRowChange &&
        !isColChange) ||
        isFinishEditing);

    const prop = this.propReorder.getById(sel1.colId);
    if (prop && willSelect) {
      if (
        !this.editing.isEditing &&
        prop.column === "CheckBox" &&
        !prop.isReadOnly
      ) {
        this.propCursor.setSelId(sel1.colId!);
        this.recCursor.setSelId(sel1.rowId!);
        this.aStartEditing.do();
        const value = this.dataTable.getValueById(sel1.rowId!, sel1.colId!);
        this.form.setDirtyValue(prop.id, !value);
        this.aFinishEditing.do();
        return;
      }
    }

    if (isFinishEditing) {
      this.aFinishEditing.do();
    }
    if (willSelect) {
      this.propCursor.setSelId(sel1.colId!);
      this.recCursor.setSelId(sel1.rowId!);
    }
    if (isStartEditing) {
      this.aStartEditing.do();
    }
  }*/

  @action.bound
  doSelFirst() {
    this.doByIdx(0, 0);
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

  get aFinishEditing() {
    return unpack(this.P.aFinishEditing);
  }

  get dataTable() {
    return unpack(this.P.dataTable);
  }

  get propReorder() {
    return unpack(this.P.propReorder);
  }

  get editing() {
    return unpack(this.P.editing);
  }

  get form() {
    return unpack(this.P.form);
  }
}
