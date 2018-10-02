import { computed } from "mobx";
import {
  IGridTopology,
  IGridSetup,
  IGridInteractionSelectors,
  IGridSelectors,
  IGridCursorView
} from "./types";

export class GridCursorView implements IGridCursorView {
  
  constructor(
    public gridInteractionSelectors: IGridInteractionSelectors,
    public gridViewSelectors: IGridSelectors
  ) {}

  public gridTopology: IGridTopology;
  public gridSetup: IGridSetup;

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
  get isCellSelected(): boolean {
    return this.gridInteractionSelectors.isCellSelected;
  }

  @computed
  get isCellEditing(): boolean {
    return this.gridInteractionSelectors.isCellEditing;
  }

  @computed get editingCellValue(): string | undefined {
    if(this.isCellEditing) {
      const fieldIndex = this.gridTopology.getColumnIndexById(this.editingColumnId!);
      const recordIndex = this.gridTopology.getRowIndexById(this.editingRowId!);
      return this.gridSetup.getCellValue(recordIndex, fieldIndex);
    } else {
      return;
    } 
  }
}
