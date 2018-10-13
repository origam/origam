import { action, observable, computed } from "mobx";
import { IDataTableActions, IDataTableSelectors } from "../DataTable/types";
import {
  IGridInteractionSelectors,
  IGridSelectors,
  IGridTopology,
  IGridInteractionActions,
  IGridPaneView
} from "../Grid/types";
import { IGridToolbarView } from "./types";
import { reactionRuntimeInfo } from "../utils/reaction";

export class GridToolbarView implements IGridToolbarView {
  constructor(
    public gridInteractionSelectors: IGridInteractionSelectors,
    public gridViewSelectors: IGridSelectors,
    public dataTableSelectors: IDataTableSelectors,
    public dataTableActions: IDataTableActions,
    public gridInteractionActions: IGridInteractionActions,
    public gridTopologyProvider: { gridTopology: IGridTopology }
  ) {}

  public get gridTopology() {
    return this.gridTopologyProvider.gridTopology;
  }

  @computed
  public get activeView() {
    return this.gridInteractionSelectors.activeView;
  }

  @action.bound
  public handleAddRecordClick(event: any) {
    const newRecord = this.dataTableSelectors.newRecord();
    if (this.gridInteractionSelectors.isCellSelected) {
      this.dataTableActions.putRecord(newRecord, {
        after: this.gridInteractionSelectors.selectedRowId
      });
    } else {
      const firstVisibleRowIndex = this.gridViewSelectors.visibleRowsFirstIndex;
      if (firstVisibleRowIndex > -1) {
        const firstVisibleId = this.gridTopology.getRowIdByIndex(
          firstVisibleRowIndex
        );
        this.dataTableActions.putRecord(newRecord, {
          after: firstVisibleId
        });
      } else {
        this.dataTableActions.putRecord(newRecord, {});
      }
    }
    const fieldIdToSelect = this.gridTopology.getColumnIdByIndex(0);
    this.gridInteractionActions.select(newRecord.id, fieldIdToSelect!);
    this.gridInteractionActions.editSelectedCell();
    // So it does not immediatelly cause end of editation.
    event.stopPropagation();
  }

  @action.bound
  public handleRemoveRecordClick(event: any) {
    if (!this.gridInteractionSelectors.isCellSelected) {
      return;
    }
    const rowIdToDelete = this.gridInteractionSelectors.selectedRowId!;
    const rowIdToSelect =
      this.gridTopology.getDownRowId(rowIdToDelete) ||
      this.gridTopology.getUpRowId(rowIdToDelete);
    const record = this.dataTableSelectors.getRecordById(rowIdToDelete);
    this.dataTableActions.deleteRecord(record!);
    if (rowIdToSelect) {
      this.gridInteractionActions.select(
        rowIdToSelect,
        this.gridInteractionSelectors.selectedColumnId!
      );
    }
  }

  @action.bound
  public handleSetGridViewClick(event: any): void {
    this.gridInteractionActions.setActiveView(IGridPaneView.Grid);
  }

  @action.bound
  public handleSetFormViewClick(event: any): void {
    this.gridInteractionActions.setActiveView(IGridPaneView.Form);
  }

  @action.bound public handlePrevRecordClick(event: any): void {
    reactionRuntimeInfo.add("UI", "MOUSE");
    this.gridInteractionActions.selectOneUp();
  }

  @action.bound public handleNextRecordClick(event: any): void {
    reactionRuntimeInfo.add("UI", "MOUSE");
    this.gridInteractionActions.selectOneDown();
  }
}
