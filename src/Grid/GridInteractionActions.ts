import { action, reaction } from "mobx";
import { EventObserver } from "../utils/events";
import { reactionRuntimeInfo } from "../utils/reaction";

import {
  IGridInteractionActions,
  IGridInteractionState,
  IGridInteractionSelectors
} from "./types";

export class GridInteractionActions implements IGridInteractionActions {
  constructor(
    public state: IGridInteractionState,
    public selectors: IGridInteractionSelectors
  ) {}

  // ==============================================================
  // EVENT OBSERVERS
  // ==============================================================

  public onWillEndEditing = EventObserver();
  public onWillStartEditing = EventObserver();
  public onDidEndEditing = EventObserver();
  public onDidStartEditing = EventObserver();

  // ==============================================================
  // EVENT HANDLERS
  // ==============================================================

  @action.bound
  public handleDumbEditorKeyDown(event: any) {
    const shift = event.shiftKey ? "Shift" : "";
    const methodName = `handleDumbEditorKeyDown_${shift}${event.key}`;
    console.log(methodName);
    const method = this[methodName];
    if (method) {
      method(event);
    }
    event.stopPropagation();
  }

  @action.bound
  private handleDumbEditorKeyDown_Enter(event: any) {
    this.selectOneDown();
    this.editSelectedCell();
  }

  @action.bound
  private handleDumbEditorKeyDown_ShiftEnter(event: any) {
    this.selectOneUp();
    this.editSelectedCell();
  }

  @action.bound
  private handleDumbEditorKeyDown_Tab(event: any) {
    this.selectOneRight();
    this.editSelectedCell();
  }

  @action.bound
  private handleDumbEditorKeyDown_ShiftTab(event: any) {
    this.selectOneLeft();
    this.editSelectedCell();
  }

  @action.bound
  private handleDumbEditorKeyDown_Escape(event: any) {
    this.unedit();
  }

  @action.bound
  public handleGridCellClick(
    event: any,
    cell: { rowId: string; columnId: string }
  ) {
    reactionRuntimeInfo.add("UI", "MOUSE");
    if (
      (this.selectors.isCellSelected &&
        cell.rowId === this.selectors.selectedRowId &&
        cell.columnId === this.selectors.selectedColumnId) ||
      this.selectors.isCellEditing
    ) {
      this.edit(cell.rowId, cell.columnId);
    }
    this.select(cell.rowId, cell.columnId);
  }

  @action.bound
  public handleGridNoCellClick(event: any) {
    this.unedit();
  }

  @action.bound
  public handleGridOutsideClick(event: any) {
    this.unedit();
    // this.unselect();
  }

  @action.bound
  public handleGridKeyDown(event: any) {
    reactionRuntimeInfo.add("UI", "KEYBOARD");
    const shift = event.shiftKey ? "Shift" : "";
    const methodName = `handleGridKeyDown_${shift}${event.key}`;
    const method = this[methodName];
    if (method) {
      method(event);
      event.preventDefault();
    }
  }

  @action.bound
  private handleGridKeyDown_ArrowLeft(event: any) {
    this.selectOneLeft();
  }

  @action.bound
  private handleGridKeyDown_ArrowRight(event: any) {
    this.selectOneRight();
  }

  @action.bound
  private handleGridKeyDown_ArrowUp(event: any) {
    this.selectOneUp();
  }

  @action.bound
  private handleGridKeyDown_ArrowDown(event: any) {
    this.selectOneDown();
  }

  @action.bound
  private handleGridKeyDown_Enter(event: any) {
    this.selectOneDown();
  }

  @action.bound
  private handleGridKeyDown_ShiftEnter(event: any) {
    this.selectOneUp();
  }

  @action.bound
  private handleGridKeyDown_Tab(event: any) {
    this.selectOneRight();
  }

  @action.bound
  private handleGridKeyDown_ShiftTab(event: any) {
    this.selectOneLeft();
  }

  // ==============================================================
  // EXECUTIVE
  // ==============================================================

  @action.bound
  public edit(rowId: string | undefined, columnId: string | undefined) {
    this.select(rowId, columnId);
    this.state.setEditing(rowId, columnId);
  }

  @action.bound
  public unedit() {
    this.state.setEditing(undefined, undefined);
  }

  @action.bound
  public select(rowId: string | undefined, columnId: string | undefined) {
    this.state.setSelected(rowId, columnId);
  }

  @action.bound
  public unselect() {
    this.state.setSelected(undefined, undefined);
  }

  @action.bound
  public selectOneLeft() {
    if (this.selectors.isCellSelected) {
      const newColumnId = this.selectors.getLeftColumnId(
        this.selectors.selectedColumnId!
      );
      this.state.setSelectedColumn(newColumnId);
    }
  }

  @action.bound
  public selectOneRight() {
    if (this.selectors.isCellSelected) {
      const newColumnId = this.selectors.getRightColumnId(
        this.selectors.selectedColumnId!
      );
      this.state.setSelectedColumn(newColumnId);
    }
  }

  @action.bound
  public selectOneUp() {
    if (this.selectors.isCellSelected) {
      const newRowId = this.selectors.getUpRowId(this.selectors.selectedRowId!);
      this.state.setSelectedRow(newRowId);
    }
  }

  @action.bound
  public selectOneDown() {
    if (this.selectors.isCellSelected) {
      const newRowId = this.selectors.getDownRowId(
        this.selectors.selectedRowId!
      );
      this.state.setSelectedRow(newRowId);
    }
  }

  @action.bound
  public editOneLeft() {
    throw new Error("Not yet implemented.");
  }

  @action.bound
  public editOneRight() {
    throw new Error("Not yet implemented.");
  }

  @action.bound
  public editOneTop() {
    throw new Error("Not yet implemented.");
  }

  @action.bound
  public editOneBottom() {
    throw new Error("Not yet implemented.");
  }

  @action.bound
  public editSelectedCell() {
    this.edit(this.selectors.selectedRowId, this.selectors.selectedColumnId);
  }
}
