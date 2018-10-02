import { action, observable } from "mobx";
import { IDataTableActions, IDataTableSelectors } from "../DataTable/types";
import {
  IGridInteractionSelectors,
  IGridSelectors,
  IGridTopology,
  IGridInteractionActions
} from "../Grid/types";
import { IGridToolbarView } from "./types";

export class GridToolbarView implements IGridToolbarView {
  constructor(
    public gridInteractionSelectors: IGridInteractionSelectors,
    public gridViewSelectors: IGridSelectors,
    public dataTableSelectors: IDataTableSelectors,
    public dataTableActions: IDataTableActions,
    public gridInteractionActions: IGridInteractionActions
  ) {}

  @observable.ref
  public gridTopology: IGridTopology;

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
}
