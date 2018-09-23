import { computed } from "mobx";
import { IGridInteractionState, IGridTopology, IGridInteractionSelectors } from "./types";

export class GridInteractionSelectors implements IGridInteractionSelectors {
  constructor(
    public state: IGridInteractionState,
    public gridTopology: IGridTopology
  ) {}

  // ==============================================================
  // DATA ROUTING
  // ==============================================================

  public getUpRowId(rowId: string) {
    return this.gridTopology.getUpRowId(rowId);
  }

  public getDownRowId(rowId: string) {
    return this.gridTopology.getDownRowId(rowId);
  }

  public getLeftColumnId(columnId: string) {
    return this.gridTopology.getLeftColumnId(columnId);
  }

  public getRightColumnId(columnId: string) {
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
