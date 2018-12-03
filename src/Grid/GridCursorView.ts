import { computed, action } from "mobx";
import {
  IGridTopology,
  IGridSetup,
  IGridInteractionSelectors,
  IGridSelectors,
  IGridCursorView
} from "./types";
import {
  IDataTableSelectors,
  IDataTableActions,
  ICellValue,
  IDataTableRecord,
  IDataTableFieldStruct
} from "../DataTable/types";

export class GridCursorView implements IGridCursorView {
  constructor(
    public gridInteractionSelectors: IGridInteractionSelectors,
    public gridViewSelectors: IGridSelectors,
    public dataTableSelectors: IDataTableSelectors,
    public dataTableActions: IDataTableActions,
    public gridSetupProvider: { gridSetup: IGridSetup },
    public gridTopologyProvider: { gridTopology: IGridTopology }
  ) {}

  public get gridTopology() {
    return this.gridTopologyProvider.gridTopology;
  }

  public get gridSetup() {
    return this.gridSetupProvider.gridSetup;
  }

  @computed
  get currentRowHeight() {
    const { selectedRowId } = this.gridInteractionSelectors;
    if (!selectedRowId) {
      return 0;
    }
    const rowHeight = this.gridSetup.getRowHeight(
      this.gridTopology.getRowIndexById(selectedRowId)
    );
    return rowHeight;
  }

  @computed
  get currentColumnWidth() {
    const { selectedColumnId } = this.gridInteractionSelectors;
    if (!selectedColumnId) {
      return 0;
    }
    const columnWidth = this.gridSetup.getColumnWidth(
      this.gridTopology.getColumnIndexById(selectedColumnId)
    );
    return columnWidth;
  }

  @computed
  get currentRowTop() {
    const { selectedRowId } = this.gridInteractionSelectors;
    if (!selectedRowId) {
      return 0;
    }
    const rowTop = this.gridSetup.getRowTop(
      this.gridTopology.getRowIndexById(selectedRowId)
    );
    return rowTop;
  }

  @computed
  get currentColumnLeft() {
    const { selectedColumnId } = this.gridInteractionSelectors;
    if (!selectedColumnId) {
      return 0;
    }
    const columnLeft = this.gridSetup.getColumnLeft(
      this.gridTopology.getColumnIndexById(selectedColumnId)
    );
    return columnLeft;
  }

  @computed
  get isCurrentCellFixed() {
    const selectedColumnId = this.gridInteractionSelectors.selectedColumnId;
    if (!selectedColumnId) {
      return false;
    }
    const selectedColumnIndex = this.gridTopology.getColumnIndexById(
      selectedColumnId
    );
    return selectedColumnIndex < this.gridSetup.fixedColumnCount;
  }

  @computed
  get rowCursorClientTop() {
    return this.currentRowTop - this.gridViewSelectors.scrollTop;
  }

  @computed
  get rowCursorClientHeight() {
    return this.currentRowHeight;
  }

  @computed
  get fixedRowCursorClientLeft() {
    return 0; // - this.gridViewSelectors.scrollLeft;
  }

  @computed
  get movingRowCursorClientLeft() {
    return this.gridViewSelectors.fixedColumnsTotalWidth; // - this.gridViewSelectors.scrollLeft;
  }

  @computed
  get cellCursorClientTop() {
    return 0;
  }

  @computed
  get cellCursorClientHeight() {
    return this.currentRowHeight;
  }

  @computed
  get movingCellCursorClientLeft() {
    return (
      this.currentColumnLeft -
      this.gridViewSelectors.scrollLeft -
      this.gridViewSelectors.fixedColumnsTotalWidth
    );
  }

  @computed
  get fixedCellCursorClientLeft() {
    return this.currentColumnLeft;
  }

  @computed
  get cellCursorClientWidth() {
    return this.currentColumnWidth;
  }

  @computed
  get fixedRowCursorClientWidth() {
    return this.gridViewSelectors.fixedColumnsTotalWidth;
  }

  @computed
  get movingRowCursorClientWidth() {
    return (
      this.gridViewSelectors.innerWidth -
      this.gridViewSelectors.fixedColumnsTotalWidth
    );
  }

  @computed
  get fixedRowCursorDisplayed() {
    return this.gridInteractionSelectors.isCellSelected;
  }

  @computed
  get movingRowCursorDisplayed() {
    return this.gridInteractionSelectors.isCellSelected;
  }

  @computed
  get fixedCellCursorDisplayed() {
    return (
      this.gridInteractionSelectors.isCellSelected && this.isCurrentCellFixed
    );
  }

  @computed
  get movingCellCursorDisplayed() {
    return (
      this.gridInteractionSelectors.isCellSelected && !this.isCurrentCellFixed
    );
  }

  @computed
  get fixedCellCursorStyle() {
    return {
      width: this.cellCursorClientWidth,
      height: this.cellCursorClientHeight,
      top: this.cellCursorClientTop,
      left: this.fixedCellCursorClientLeft
    };
  }

  @computed
  get movingCellCursorStyle() {
    return {
      width: this.cellCursorClientWidth,
      height: this.cellCursorClientHeight,
      top: this.cellCursorClientTop,
      left: this.movingCellCursorClientLeft
    };
  }

  @computed
  get fixedRowCursorStyle() {
    return {
      width: this.fixedRowCursorClientWidth,
      height: this.rowCursorClientHeight,
      top: this.rowCursorClientTop,
      left: this.fixedRowCursorClientLeft
    };
  }

  @computed public get cursorPosition() {
    if(this.isCurrentCellFixed) {
      return this.fixedCellCursorStyle;
    } else {
      return this.movingCellCursorStyle;
    }
  }

  @computed
  get movingRowCursorStyle() {
    return {
      width: this.movingRowCursorClientWidth,
      height: this.rowCursorClientHeight,
      top: this.rowCursorClientTop,
      left: this.movingRowCursorClientLeft
    };
  }

  @computed
  get selectedRowId(): string | undefined {
    return this.gridInteractionSelectors.selectedRowId;
  }

  @computed
  get selectedColumnId(): string | undefined {
    return this.gridInteractionSelectors.selectedRowId;
  }

  @computed
  get editingRowId(): string | undefined {
    return this.gridInteractionSelectors.editingRowId;
  }

  @computed
  get editingColumnId(): string | undefined {
    return this.gridInteractionSelectors.editingColumnId;
  }

  @computed
  public get editingRecord(): IDataTableRecord | undefined {
    if (!this.editingRowId) {
      return undefined;
    }
    return this.dataTableSelectors.getRecordById(this.editingRowId);
  }

  @computed
  public get editingField(): IDataTableFieldStruct | undefined {
    if (!this.editingColumnId) {
      return undefined;
    }
    return this.dataTableSelectors.getFieldById(
      this.editingColumnId
    ) as IDataTableFieldStruct;
  }

  @computed
  get isCellSelected(): boolean {
    return this.gridInteractionSelectors.isCellSelected;
  }

  @computed
  get isCellEditing(): boolean {
    return this.gridInteractionSelectors.isCellEditing;
  }

  @computed
  get editingOriginalCellValue(): ICellValue | undefined {
    console.log(Array.from(this.dataTableSelectors.fullRecords));
    if (this.isCellEditing) {
      const record = this.dataTableSelectors.getRecordById(
        this.gridInteractionSelectors.editingRowId!
      );
      const field = this.dataTableSelectors.getFieldById(
        this.gridInteractionSelectors.editingColumnId!
      );
      return this.dataTableSelectors.getOriginalValue(record!, field!);
    } else {
      return;
    }
  }

  @action.bound
  public handleDataCommit(
    dirtyValue: string,
    editingRecordId: string,
    editingFieldId: string
  ): void {
    console.log("Commit", dirtyValue, editingRecordId, editingFieldId);
    const record = this.dataTableSelectors.getRecordById(editingRecordId);
    const field = this.dataTableSelectors.getFieldById(editingFieldId);
    this.dataTableActions.setDirtyCellValue(record!, field!, dirtyValue);
  }
}
