import { IGridDimensions, IGridCursorPos, IFixedColumnSettings } from "./types";
import { computed } from "mobx";

export class GridDimensions implements IGridDimensions {
  constructor(
    private globalGridDimensions: IGridDimensions,
    private fixedColumnSettings: IFixedColumnSettings,
    private isFixedPart: boolean
  ) {}

  @computed public get rowCount() {
    return this.globalGridDimensions.rowCount;
  }

  @computed public get columnCount() {
    return this.isFixedPart
      ? this.fixedColumnSettings.fixedColumnCount
      : this.globalGridDimensions.columnCount 
  }

  @computed public get contentWidth() {
    return this.getColumnRight(this.columnCount - 1) - this.getColumnLeft(0);
  }

  @computed public get contentHeight() {
    return this.globalGridDimensions.contentHeight;
  }

  public getColumnLeft(columnIndex: number): number {
    return this.globalGridDimensions.getColumnLeft(
      this.isFixedPart
        ? columnIndex
        : columnIndex - this.fixedColumnSettings.fixedColumnCount
    );
  }

  public getColumnWidth(columnIndex: number): number {
    return this.globalGridDimensions.getColumnWidth(
      this.isFixedPart
        ? columnIndex
        : columnIndex - this.fixedColumnSettings.fixedColumnCount
    );
  }

  public getColumnRight(columnIndex: number): number {
    return this.globalGridDimensions.getColumnRight(
      this.isFixedPart
        ? columnIndex
        : columnIndex - this.fixedColumnSettings.fixedColumnCount
    );
  }

  public getRowTop(rowIndex: number): number {
    return this.globalGridDimensions.getRowTop(rowIndex);
  }
  public getRowHeight(rowIndex: number): number {
    return this.globalGridDimensions.getRowHeight(rowIndex);
  }

  public getRowBottom(rowIndex: number): number {
    return this.globalGridDimensions.getRowBottom(rowIndex);
  }
}

export class GridCursorPos implements IGridCursorPos {
  constructor(
    private gridCursorPos: IGridCursorPos,
    private fixedColumnSettings: IFixedColumnSettings,
    private isFixedPart: boolean
  ) {}

  @computed public get selectedRowIndex(): number | undefined {
    return this.gridCursorPos.selectedRowIndex;
  }

  @computed public get selectedColumnIndex(): number | undefined {
    return this.gridCursorPos.selectedColumnIndex;
  }
}
