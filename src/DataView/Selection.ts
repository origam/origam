import { computed } from "mobx";
import { IRecCursor } from "./types/IRecCursor";
import { IPropCursor } from "./types/IPropCursor";
import { IPropReorder } from "./types/IPropReorder";
import { IDataTable } from "./types/IDataTable";

export interface ISelection {
  selRowId: string | undefined;
  selColId: string | undefined;
  selRowIdx: number | undefined;
  selColIdx: number | undefined;
  isSelectedCellByIdx({
    colIdx,
    rowIdx
  }: {
    colIdx: number | undefined;
    rowIdx: number | undefined;
  }): boolean;
}

export class Selection implements ISelection {
  constructor(
    public P: {
      propCursor: IPropCursor;
      propReorder: IPropReorder;
      recCursor: IRecCursor;
      dataTable: IDataTable;
    }
  ) {}

  isSelectedCellByIdx({
    colIdx,
    rowIdx
  }: {
    colIdx: number | undefined;
    rowIdx: number | undefined;
  }): boolean {
    return colIdx === this.selColIdx && rowIdx === this.selRowIdx;
  }

  @computed get selRowId(): string | undefined {
    return this.P.recCursor.selId;
  }

  @computed get selColId(): string | undefined {
    return this.P.propCursor.selId;
  }

  @computed get selColIdx() {
    return this.P.propCursor.selId
      ? this.P.propReorder.getIndexById(this.P.propCursor.selId)
      : undefined;
  }

  @computed get selRowIdx() {
    return this.P.recCursor.selId
      ? this.P.dataTable.getRecordIndexById(this.P.recCursor.selId)
      : undefined;
  }
}
