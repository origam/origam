import { observable, action } from "mobx";
import { IGridInteractionState, GridViewType } from "./types";

export class GridInteractionState implements IGridInteractionState {

  @observable
  public activeView: GridViewType = GridViewType.Grid;

  @observable
  public selectedRowId: string | undefined;

  @observable
  public selectedColumnId: string | undefined;

  @observable
  public editingRowId: string | undefined;

  @observable
  public editingColumnId: string | undefined;

  @observable
  public columnWidths = new Map();

  @action.bound
  public setActiveView(view: GridViewType): void {
    this.activeView = view;
  }

  @action.bound
  public setEditing(rowId: string | undefined, columnId: string | undefined) {
    this.editingRowId = rowId;
    this.editingColumnId = columnId;
  }

  @action.bound
  public setSelected(rowId: string, columnId: string) {
    this.selectedRowId = rowId;
    this.selectedColumnId = columnId;
  }

  @action.bound
  public setSelectedRow(rowId: string) {
    this.selectedRowId = rowId;
  }

  @action.bound
  public setSelectedColumn(columnId: string) {
    this.selectedColumnId = columnId;
  }

  @action.bound
  public clearEditing() {
    this.setEditing(undefined, undefined);
  }
}
