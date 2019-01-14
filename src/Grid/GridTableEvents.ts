import { action } from "mobx";
import * as KEY from "../utils/keys";
import { IDataCursorState, IGridTableEvents } from "./types2";
import { EventObserver } from "src/utils/events";

export class GridTableEvents implements IGridTableEvents {
  constructor(private dataCursorState: IDataCursorState) {}

  public onCursorMovementFinished = EventObserver();

  @action.bound
  public handleCellClick(event: any, recordId: string, fieldId: string) {
    event.stopPropagation();
    if (
      this.dataCursorState.isCellSelected(recordId, fieldId) ||
      this.dataCursorState.isEditing
    ) {
      // this.dataCursorState.finishEditing();
      this.dataCursorState.selectCell(recordId, fieldId);
      // this.dataCursorState.editSelected();
    }
    this.dataCursorState.selectCell(recordId, fieldId);
    this.onCursorMovementFinished.trigger();
  }

  @action.bound
  public handleNoCellClick(event: any) {
    event.stopPropagation();
    // this.dataCursorState.finishEditing();
  }

  @action.bound
  public handleOutsideClick(event: any) {
    event.stopPropagation();
    // this.dataCursorState.finishEditing();
  }

  @action.bound
  public handleGridKeyDown(event: any) {
    event.stopPropagation();
    switch (event.key) {
      case KEY.ArrowUp:
        event.preventDefault();
        this.dataCursorState.selectPrevRow();
        break;
      case KEY.ArrowDown:
        event.preventDefault();
        this.dataCursorState.selectNextRow();
        break;
      case KEY.ArrowLeft:
        event.preventDefault();
        this.dataCursorState.selectPrevColumn();
        break;
      case KEY.ArrowRight:
        event.preventDefault();
        this.dataCursorState.selectNextColumn();
        break;
      case KEY.Enter:
        event.preventDefault();
        if (event.shiftKey) {
          this.dataCursorState.selectPrevRow();
        } else {
          this.dataCursorState.selectNextRow();
        }
        break;
      case KEY.Tab:
        event.preventDefault();
        if (event.shiftKey) {
          this.dataCursorState.selectPrevRow();
        } else {
          this.dataCursorState.selectNextRow();
        }
        break;
      case KEY.F2:
        event.preventDefault();
        this.dataCursorState.editSelected();
        break;
    }
    this.onCursorMovementFinished.trigger();
  }

  @action.bound
  public handleDefaultEditorKeyDown(event: any) {
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
    this.onCursorMovementFinished.trigger();
  }

}
