import { computed } from "mobx";
import {
  IGridInteractionState,
  IGridTopology,
  IGridInteractionSelectors,
  GridViewType,
  IFormTopology
} from "./types";
import { IRecordId, IFieldId } from "../DataTable/types";

export class GridInteractionSelectors implements IGridInteractionSelectors {
  constructor(
    public state: IGridInteractionState,
    public gridTopologyProvider: { gridTopology: IGridTopology },
    public formTopologyProvider: { formTopology: IFormTopology }
  ) {}

  public get gridTopology() {
    return this.gridTopologyProvider.gridTopology;
  }

  public get formTopology() {
    return this.formTopologyProvider.formTopology;
  }

  @computed
  public get activeView(): GridViewType {
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
    if (this.activeView === GridViewType.Grid) {
      return this.gridTopology.getLeftColumnId(columnId);
    } else if (this.activeView === GridViewType.Form) {
      return this.formTopology.getPrevFieldId(columnId);
    }
    return;
  }

  public getRightColumnId(columnId: IFieldId) {
    if (this.activeView === GridViewType.Grid) {
      return this.gridTopology.getRightColumnId(columnId);
    } else if (this.activeView === GridViewType.Form) {
      return this.formTopology.getNextFieldId(columnId);
    }
    return;
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
