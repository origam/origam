import { computed } from "mobx";
import {
  IGridInteractionState,
  IGridTopology,
  IGridInteractionSelectors,
  IGridPaneView
} from "./types";
import { IRecordId, IFieldId } from "../DataTable/types";

export class GridInteractionSelectors implements IGridInteractionSelectors {
  constructor(
    public state: IGridInteractionState,
    public gridTopologyProvider: { gridTopology: IGridTopology }
  ) {}

  public get gridTopology() {
    return this.gridTopologyProvider.gridTopology;
  }

  @computed
  public get activeView(): IGridPaneView {
    return this.state.activeView;
  }

  // ==============================================================
  // DATA ROUTING
  // ==============================================================

  public getUpRowId(rowId: IRecordId) {
    return this.gridTopology.getUpRowId(rowId);
  }

  public getDownRowId(rowId: IRecordId) {
    return this.gridTopology.getDownRowId(rowId);
  }

  public getLeftColumnId(columnId: IFieldId) {
    return this.gridTopology.getLeftColumnId(columnId);
  }

  public getRightColumnId(columnId: IFieldId) {
    return this.gridTopology.getRightColumnId(columnId);
  }

  // ==============================================================
  // COMPUTED OUTPUTS
  // ==============================================================

  @computed
  public get isScrollDisable() {
    return this.isCellEditing;
  }

  @computed
  public get editingColumnId() {
    return this.state.editingColumnId;
  }

  @computed
  public get editingRowId() {
    return this.state.editingRowId;
  }

  @computed
  public get selectedColumnId() {
    return this.state.selectedColumnId;
  }

  @computed
  public get selectedRowId() {
    return this.state.selectedRowId;
  }

  @computed
  public get isCellSelected() {
    return (
      this.selectedRowId !== undefined && this.selectedColumnId !== undefined
    );
  }

  @computed
  public get isCellEditing() {
    return (
      this.editingRowId !== undefined && this.editingColumnId !== undefined
    );
  }
}
