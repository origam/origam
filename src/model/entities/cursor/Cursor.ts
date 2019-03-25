import { ICursor } from "./types/ICursor";
import { observable, computed } from "mobx";
import { IDataTable } from "../data/types/IDataTable";

export class Cursor implements ICursor {
  constructor(public dataTable: IDataTable) {}

  @observable selRowId: string | undefined;
  @observable selColId: string | undefined;
  @observable isEditing: boolean = false;
  @observable isSelected: boolean = false;

  @computed get selRowIdx(): number | undefined {
    return this.selRowId
      ? this.dataTable.getRowIndexById(this.selRowId)
      : undefined;
  }

  @computed get selColumnIdx(): number | undefined {
    return this.selColId
      ? this.dataTable.getColumnIndexById(this.selColId)
      : undefined;
  }

  isCurrentSelection(
    rowId: string | undefined,
    colId: string | undefined
  ): boolean {
    return this.selRowId === rowId && this.selColId === colId;
  }

  isSameRow(rowId: string | undefined) {
    return this.selRowId === rowId;
  }

  isSameColumn(colId: string | undefined) {
    return this.selColId === colId;
  }

  willSelect(rowId: string | undefined, colId: string | undefined) {
    return rowId && colId;
  }

  selectCell(rowId: string | undefined, colId: string | undefined): void {
    if (this.isEditing || this.isCurrentSelection(rowId, colId)) {
      /*
        When we are already editing or we clicked the same cell which is 
        already selected, this action actually means editing.
      */
      this.editCell(rowId, colId);
    } else {
      this.setSelection(rowId, colId);
      this.ensureSelectionVisible();
    }
  }

  selectCellByIdx(
    rowIdx: number | undefined,
    colIdx: number | undefined
  ): void {
    this.selectCell(
      rowIdx !== undefined ? this.dataTable.getRowIdByIndex(rowIdx) : undefined,
      colIdx !== undefined
        ? this.dataTable.getColumnIdByIndex(colIdx)
        : undefined
    );
  }

  editCell(rowId: string | undefined, colId: string | undefined): void {
    /*
      Stop editing when we are editing now and selected row is going to change or
      selected cell is no selection.
    */
    const isStopEditing =
      this.isEditing &&
      (!this.isSameRow(rowId) || !this.willSelect(rowId, colId));

    /*
   Start editing when we are editing now, target row changes and resulting 
   selection is a selection OR
   We are not editing and target selection is a selection.
 */
    const isStartEditing =
      (this.isEditing &&
        !this.isSameRow(rowId) &&
        this.willSelect(rowId, colId)) ||
      this.willSelect(rowId, colId); // TODO: CHECK!!!!!

    /* 
   Finish editing at the old place, change the selection, reatart it on a 
   new place, fix form focus and cell visibility.
 */
    if (isStopEditing) {
      this.finishEditing();
    }
    this.setSelection(rowId, colId);
    if (isStartEditing) {
      this.startEditing();
    }
    if (this.isEditing) {
      this.focusColumn(colId!);
    }
    this.ensureSelectionVisible();
  }

  selectRow(rowId: string | undefined): void {
    this.selectCell(rowId, this.selColId);
  }

  selectColumn(colId: string | undefined): void {
    this.selectCell(this.selRowId, colId);
  }

  selectClosestRowToId(rowId: string): void {
    throw new Error("Method not implemented.");
  }

  selectNextRow(): void {
    if (this.isSelected) {
      this.selectRow(this.dataTable.getRecordIdAfterId(this.selRowId!));
    }
  }

  selectPrevRow(): void {
    if (this.isSelected) {
      this.selectRow(this.dataTable.getRecordIdBeforeId(this.selRowId!));
    }
  }

  selectNextColumn(): void {
    if (this.isSelected) {
      this.selectColumn(this.dataTable.getPropertyIdAfterId(this.selColId!));
    }
  }

  selectPrevColumn(): void {
    if (this.isSelected) {
      this.selectColumn(this.dataTable.getPropertyIdBeforeId(this.selColId!));
    }
  }

  startEditRow(rowId: string): void {
    throw new Error("Method not implemented.");
  }

  setSelection(rowId: string | undefined, colId: string | undefined): void {
    this.selRowId = rowId;
    this.selColId = colId;
  }

  ensureSelectionVisible() {
    return;
    throw new Error("Method not implemented.");
  }

  finishEditing() {
    throw new Error("Method not implemented.");
    this.isEditing = false;
  }

  startEditing() {
    throw new Error("Method not implemented.");
    this.isEditing = true;
  }

  focusColumn(colId: string) {
    throw new Error("Method not implemented.");
  }
}
