import { decorate } from "mobx";
import { IGridState, IGridSelectors, IGridSetup, IGridTopology, ICellRenderer } from "./types";
import { rangeQuery } from "../utils/arrays";

export class GridSelectors implements IGridSelectors {
  constructor(
    public state: IGridState,
    public setup: IGridSetup,
    public topology: IGridTopology
  ) {}

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
    return this.setup.getRowTop(rowIndex);
  }

  public getRowBottom(rowIndex: number): number {
    return this.setup.getRowBottom(rowIndex);
  }

  public getRowHeight(rowIndex: number): number {
    return this.setup.getRowHeight(rowIndex);
  }

  public getColumnLeft(columnIndex: number): number {
    return this.setup.getColumnLeft(columnIndex);
  }

  public getColumnRight(columIndex: number): number {
    return this.setup.getColumnRight(columIndex);
  }

  public getColumnWidth(columnIndex: number): number {
    return this.setup.getColumnWidth(columnIndex);
  }

  public getColumnId(columnIndex: number): string {
    return this.topology.getColumnIdByIndex(columnIndex);
  }

  get contentWidth(): number {
    const columnCount = this.setup.columnCount;
    if (columnCount === 0) {
      return 0;
    }
    return this.getColumnRight(columnCount - 1) - this.getColumnLeft(0);
  }

  get contentHeight(): number {
    const rowCount = this.setup.rowCount;
    if (rowCount === 0) {
      return 0;
    }
    return this.getRowBottom(rowCount - 1) - this.getRowTop(0);
  }

  get visibleRowsRange() {
    return rangeQuery(
      i => this.getRowBottom(i),
      i => this.getRowTop(i),
      this.setup.rowCount,
      this.scrollTop,
      this.scrollTop + this.innerHeight
    );
  }

  get visibleRowsFirstIndex() {
    return this.visibleRowsRange.fgte;
  }

  get visibleRowsLastIndex() {
    return this.visibleRowsRange.llte;
  }

  get visibleColumnsRange() {
    return rangeQuery(
      i => this.getColumnRight(i),
      i => this.getColumnLeft(i),
      this.setup.columnCount,
      this.scrollLeft + this.fixedColumnsTotalWidth,
      this.scrollLeft + this.innerWidth
    );
  }

  get visibleColumnsFirstIndex() {
    return this.visibleColumnsRange.fgte;
  }

  get visibleColumnsLastIndex() {
    return this.visibleColumnsRange.llte;
  }

  public get fixedColumnCount(): number {
    return this.setup.fixedColumnCount;
  }

  public get columnHeadersOffsetLeft() {
    return -this.state.scrollLeft;
  }

  public get elmRoot(): HTMLDivElement | null {
    return this.state.elmRoot;
  }

  public get columnCount(): number {
    return this.setup.columnCount;
  }

  get movingColumnsTotalWidth() {
    let totWidth = 0;
    const columnCount = this.columnCount;
    for (let i = this.fixedColumnCount; i < columnCount; i++) {
      totWidth = totWidth + this.getColumnRight(i) - this.getColumnLeft(i);
    }
    return totWidth;
  }

  get fixedColumnsTotalWidth() {
    let totWidth = 0;
    for (let i = 0; i < this.fixedColumnCount; i++) {
      totWidth = totWidth + this.getColumnRight(i) - this.getColumnLeft(i);
    }
    return totWidth;
  }

  public fixedShiftedColumnDimensions(
    left: number,
    width: number,
    columnIndex: number
  ): { left: number; width: number } {
    if (columnIndex < this.fixedColumnCount) {
      return { left, width };
    } else {
      const leftOverflow = Math.max(
        this.fixedColumnsTotalWidth - (left - this.scrollLeft),
        0
      );
      return {
        left: Math.max(left - this.scrollLeft, this.fixedColumnsTotalWidth),
        width: Math.max(width - leftOverflow, 0)
      };
    }
  }

  public get cellRenderer(): ICellRenderer {
    return this.state.cellRenderer;
  }

  public get scrollLeft(): number {
    return this.state.scrollLeft;
  }

  public get scrollTop(): number {
    return this.state.scrollTop;
  }

  public get canvasContext(): CanvasRenderingContext2D | null {
    return this.state.canvasContext;
  }

  public get elmScroller(): HTMLDivElement | null {
    return this.state.elmScroller;
  }

  public get onOutsideClick(): ((event: any) => void) | undefined {
    return this.state.onOutsideClick;
  }

  public get onScroll(): ((event: any) => void) | undefined {
    return this.state.onScroll;
  }

  public get onKeyDown(): ((event: any) => void) | undefined {
    return this.state.onKeyDown;
  }
}

decorate(GridSelectors, {});
