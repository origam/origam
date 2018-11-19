import { action, IReactionDisposer, reaction, computed } from "mobx";
import { EventObserver } from "../utils/events";
import { reactionRuntimeInfo } from "../utils/reaction";
import {
  IGridActions,
  IGridConfiguration,
  IGridInteractionActions,
  IGridInteractionSelectors,
  IGridInteractionState,
  GridViewType,
  IGridSelectors,
  IFormActions
} from "./types";

export class GridInteractionActions implements IGridInteractionActions {
  constructor(
    public state: IGridInteractionState,
    public selectors: IGridInteractionSelectors,
    public gridViewActions: IGridActions,
    public gridViewSelectors: IGridSelectors,
    public formViewActions: IFormActions,
    public configuration: IGridConfiguration
  ) {}

  @computed
  public get gridTopology() {
    return this.configuration.gridTopology;
  }

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
    if (this.selectors.activeView === GridViewType.Grid) {
      this.selectOneDown();
      this.editSelectedCell();
    } else if(this.selectors.activeView === GridViewType.Form) {
      this.unedit();
    }
    event.preventDefault();
  }

  @action.bound
  private handleDumbEditorKeyDown_ShiftEnter(event: any) {
    if (this.selectors.activeView === GridViewType.Grid) {
      this.selectOneUp();
      this.editSelectedCell();
    } else if(this.selectors.activeView === GridViewType.Form) {
      this.unedit();
    }
    event.preventDefault();
  }

  @action.bound
  private handleDumbEditorKeyDown_Tab(event: any) {
    this.selectOneRight();
    this.editSelectedCell();
    event.preventDefault();
  }

  @action.bound
  private handleDumbEditorKeyDown_ShiftTab(event: any) {
    this.selectOneLeft();
    this.editSelectedCell();
    event.preventDefault();
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
    if (this.selectors.activeView === GridViewType.Grid) {
      this.unedit();
    }
    // this.unselect();
  }

  @action.bound
  public handleGridKeyDown(event: any) {
    if (this.selectors.activeView === GridViewType.Grid) {
      reactionRuntimeInfo.add("UI", "KEYBOARD");
      const shift = event.shiftKey ? "Shift" : "";
      const methodName = `handleGridKeyDown_${shift}${event.key}`;
      const method = this[methodName];
      if (method) {
        method(event);
        event.preventDefault();
      }
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

  @action.bound
  private handleGridKeyDown_F2(event: any) {
    this.editSelectedCell();
  }

  @action.bound
  public handleFormFieldClick(event: any, field: { fieldId: string }): void {
    reactionRuntimeInfo.add("UI", "MOUSE");
    console.log(field, this.selectors.selectedColumnId);
    if (
      (this.selectors.isCellSelected &&
        field.fieldId === this.selectors.selectedColumnId) ||
      this.selectors.isCellEditing
    ) {
      this.edit(this.selectors.selectedRowId, field.fieldId);
    }
    this.select(this.selectors.selectedRowId, field.fieldId);
  }

  @action.bound
  public handleFormKeyDown(event: any): void {
    switch (event.key) {
      case "Tab":
        if (event.shiftKey) {
          this.selectOneLeft();
        } else {
          this.selectOneRight();
        }
        event.preventDefault();
        break;
      case "Enter":
        if (event.shiftKey) {
          this.selectOneLeft();
        } else {
          this.selectOneRight();
        }
        event.preventDefault();
        break;
      case "F2":
        this.editSelectedCell();
        event.preventDefault();
        break;
    }
  }

  // ==============================================================
  // EXECUTIVE
  // ==============================================================

  private reGridRootFocuser: IReactionDisposer;

  @action.bound
  public start() {
    this.reGridRootFocuser = reaction(
      () => this.selectors.isCellEditing,
      () => {
        if (!this.selectors.isCellEditing) {
          setTimeout(() => {
            if (this.selectors.activeView === GridViewType.Grid) {
              this.gridViewActions.focusRoot();
            } else if (this.selectors.activeView === GridViewType.Form) {
              this.formViewActions.focusRoot();
            }
          }, 10);
        }
      }
    );
  }

  @action.bound
  public stop() {
    this.reGridRootFocuser && this.reGridRootFocuser();
  }

  @action.bound
  public setActiveView(view: GridViewType): void {
    this.state.setActiveView(view);
  }

  @action.bound
  public edit(rowId: string | undefined, columnId: string | undefined) {
    console.log("Edit", rowId, columnId);
    this.select(rowId, columnId);
    this.state.setEditing(rowId, columnId);
  }

  @action.bound
  public unedit() {
    this.state.setEditing(undefined, undefined);
  }

  @action.bound
  public selectFirst(): void {
    const columnIndex = this.gridViewSelectors.visibleColumnsFirstIndex;
    const rowIndex = this.gridViewSelectors.visibleRowsFirstIndex;
    if (columnIndex === -1 || rowIndex === -1) {
      return;
    }
    this.select(
      this.gridTopology.getRowIdByIndex(rowIndex),
      this.gridTopology.getColumnIdByIndex(columnIndex)
    );
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
      if (newColumnId) {
        this.state.setSelectedColumn(newColumnId);
      }
    }
  }

  @action.bound
  public selectOneRight() {
    if (this.selectors.isCellSelected) {
      const newColumnId = this.selectors.getRightColumnId(
        this.selectors.selectedColumnId!
      );
      if (newColumnId) {
        this.state.setSelectedColumn(newColumnId);
      }
    }
  }

  @action.bound
  public selectOneUp(): void {
    if (this.selectors.isCellSelected) {
      const newRowId = this.selectors.getUpRowId(this.selectors.selectedRowId!);
      if (newRowId) {
        this.state.setSelectedRow(newRowId);
      }
    }
  }

  @action.bound
  public selectOneDown(): void {
    if (this.selectors.isCellSelected) {
      const newRowId = this.selectors.getDownRowId(
        this.selectors.selectedRowId!
      );
      if (newRowId) {
        this.state.setSelectedRow(newRowId);
      }
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
