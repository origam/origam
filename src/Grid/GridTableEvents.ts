import { action } from "mobx";
import * as KEY from "../utils/keys";
import {
  IDataCursorState,
  IGridTableEvents,
  IDataTableSelectors
} from "./types2";
import { EventObserver } from "src/utils/events";
import { ITableView } from "src/uiComponents/controls/GridTable2/types";

export class GridTableEvents implements IGridTableEvents {
  constructor(
    private dataCursorState: IDataCursorState,
    private dataTableSelectors: IDataTableSelectors,
    private tableView: ITableView
  ) {}

  public onCursorMovementFinished = EventObserver();

  @action.bound
  public handleCellClick(event: any, rowIndex: number, columnIndex: number) {
    if(!this.tableView.isActiveView) {
      return
    }
    event.stopPropagation();
    const recordId = this.dataTableSelectors.recordIdByIndex(rowIndex);
    const fieldId = this.dataTableSelectors.fieldIdByIndex(columnIndex);
    if(!recordId || !fieldId) {
      return
    }
    if (
      this.dataCursorState.isCellSelected(recordId, fieldId) ||
      this.dataCursorState.isEditing
    ) {
      this.dataCursorState.finishEditing();
      this.dataCursorState.selectCell(recordId, fieldId);
      this.dataCursorState.editSelected();
    }
    this.dataCursorState.selectCell(recordId, fieldId);
    this.onCursorMovementFinished.trigger();
  }

  @action.bound
  public handleNoCellClick(event: any) {
    if(!this.tableView.isActiveView) {
      return
    }
    event.stopPropagation();
    this.dataCursorState.finishEditing();
  }

  @action.bound
  public handleOutsideClick(event: any) {
    if(!this.tableView.isActiveView) {
      return
    }
    event.stopPropagation();
    this.dataCursorState.finishEditing();
  }

  @action.bound
  public handleGridKeyDown(event: any) {
    if(!this.tableView.isActiveView) {
      return
    }
    event.stopPropagation();
    switch (event.key) {
      case KEY.ArrowUp:
        event.preventDefault();
        this.dataCursorState.selectPrevRow();
        this.onCursorMovementFinished.trigger();
        break;
      case KEY.ArrowDown:
        event.preventDefault();
        this.dataCursorState.selectNextRow();
        this.onCursorMovementFinished.trigger();
        break;
      case KEY.ArrowLeft:
        event.preventDefault();
        this.dataCursorState.selectPrevColumn();
        this.onCursorMovementFinished.trigger();
        break;
      case KEY.ArrowRight:
        event.preventDefault();
        this.dataCursorState.selectNextColumn();
        this.onCursorMovementFinished.trigger();
        break;
      case KEY.Enter:
        event.preventDefault();
        if (event.shiftKey) {
          this.dataCursorState.selectPrevRow();
        } else {
          this.dataCursorState.selectNextRow();
        }
        this.onCursorMovementFinished.trigger();
        break;
      case KEY.Tab:
        event.preventDefault();
        if (event.shiftKey) {
          this.dataCursorState.selectPrevColumn();
        } else {
          this.dataCursorState.selectNextColumn();
        }
        this.onCursorMovementFinished.trigger();
        break;
      case KEY.F2:
        event.preventDefault();
        this.dataCursorState.editSelected();
        this.onCursorMovementFinished.trigger();
        break;
    }
  }

  @action.bound
  public handleDefaultEditorKeyDown(event: any) {
    if(!this.tableView.isActiveView) {
      return
    }
    event.stopPropagation();
    switch (event.key) {
      case KEY.Enter:
        event.preventDefault();
        this.dataCursorState.finishEditing();
        if (event.shiftKey) {
          this.dataCursorState.selectPrevRow();
        } else {
          this.dataCursorState.selectNextRow();
        }
        this.dataCursorState.editSelected();
        break;
      case KEY.Tab:
        event.preventDefault();
        this.dataCursorState.finishEditing();
        if (event.shiftKey) {
          this.dataCursorState.selectPrevColumn();
        } else {
          this.dataCursorState.selectNextColumn();
        }
        this.dataCursorState.editSelected();
        break;
      case KEY.Escape:
        event.preventDefault();
        this.dataCursorState.cancelEditing();
        break;
    }
  }
}
