import { decorate } from "mobx";
import { IGridState, IGridSelectors } from "./types";

export class GridSelectors implements IGridSelectors {
  constructor(public state: IGridState) {}

  public get width(): number {
    return this.state.width;
  }

  public get height(): number {
    return this.state.height;
  }

  public get innerWidth(): number {
    return this.width - 16;
  }

  public get innerHeight(): number {
    return this.height - 16;
  }

  public getRowTop(rowIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getRowBottom(rowIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getRowHeight(rowIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getColumnLeft(columnIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getColumnRight(columIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getColumnWidth(columnIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public getColumnId(columnIndex: number): number {
    throw new Error("Method not implemented.");
  }

  public get contentWidth(): number {
    throw new Error("Method not implemented.");
  }

  public get contentHeight(): number {
    throw new Error("Method not implemented.");
  }

  public get visibleRowsFirstIndex(): number {
    throw new Error("Method not implemented.");
  }

  public get visibleRowsLastIndex(): number {
    throw new Error("Method not implemented.");
  }

  public get visibleColumnsFirstIndex(): number {
    throw new Error("Method not implemented.");
  }

  public get visibleColumnsLastIndex(): number {
    throw new Error("Method not implemented.");
  }

  public get fixedColumnCount(): number {
    throw new Error("Method not implemented.");
  }

  public get columnHeadersOffsetLeft(): number {
    throw new Error("Method not implemented.");
  }

  public get elmRoot(): React.Component {
    throw new Error("Method not implemented.");
  }

  public get columnCount(): number {
    throw new Error("Method not implemented.");
  }

  public get fixedColumnsTotalWidth(): number {
    throw new Error("Method not implemented.");
  }

  public fixedShiftedColumnDimensions(
    left: number,
    width: number,
    columnIndex: number
  ): { left: number; width: number } {
    throw new Error("Method not implemented.");
  }

  public get cellRenderer(): () => {} {
    throw new Error("Method not implemented.");
  }

  public get scrollLeft(): number {
    throw new Error("Method not implemented.");
  }

  public get scrollTop(): number {
    throw new Error("Method not implemented.");
  }

  public get canvasContext(): CanvasRenderingContext2D {
    throw new Error("Method not implemented.");
  }

  public get elmScroller(): number {
    throw new Error("Method not implemented.");
  }

  public get onOutsideClick(): ((event: any) => void) | undefined {
    throw new Error("Method not implemented.");
  }

  public get onScroll(): ((event: any) => void) | undefined {
    throw new Error("Method not implemented.");
  }
}

decorate(GridSelectors, {});
