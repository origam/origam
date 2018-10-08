import { computed } from "mobx";
import {
  IGridState,
  IGridSelectors,
  IGridSetup,
  IGridTopology,
  ICellRenderer
} from "./types";
import { rangeQuery } from "../utils/arrays";
import { IFieldId } from "../DataTable/types";

export class GridSelectors implements IGridSelectors {
  constructor(
    public state: IGridState,
    public gridSetupProvider: { gridSetup: IGridSetup },
    public gridTopologyProvider: { gridTopology: IGridTopology }
  ) {}

  public get topology() {
    return this.gridTopologyProvider.gridTopology;
  }

  public get setup() {
    return this.gridSetupProvider.gridSetup;
  }

  @computed
  public get width(): number {
    return this.state.width;
  }

  @computed
  public get height(): number {
    return this.state.height;
  }

  @computed
  public get innerWidth(): number {
    return this.width - 16;
  }

  @computed
  public get innerHeight(): number {
    return this.height - 16;
  }

  @computed
  get isScrollingEnabled(): boolean {
    return this.setup.isScrollingEnabled;
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

  public getColumnId(columnIndex: number): IFieldId | undefined {
    return this.topology.getColumnIdByIndex(columnIndex);
  }

  @computed
  get contentWidth(): number {
    const columnCount = this.setup.columnCount;
    if (columnCount === 0) {
      return 0;
    }
    return this.getColumnRight(columnCount - 1) - this.getColumnLeft(0);
  }

  @computed
  public get rowCount(): number {
    return this.setup.rowCount;
  }

  @computed
  get contentHeight(): number {
    const rowCount = this.setup.rowCount;
    if (rowCount === 0) {
      return 0;
    }
    return this.getRowBottom(rowCount - 1) - this.getRowTop(0);
  }

  @computed
  get visibleRowsRange() {
    return rangeQuery(
      i => this.getRowBottom(i),
      i => this.getRowTop(i),
      this.setup.rowCount,
      this.scrollTop,
      this.scrollTop + this.innerHeight
    );
  }

  @computed
  get visibleRowsFirstIndex() {
    return this.visibleRowsRange.fgte;
  }

  @computed
  get visibleRowsLastIndex() {
    return this.visibleRowsRange.llte;
  }

  @computed
  get visibleColumnsRange() {
    return rangeQuery(
      i => this.getColumnRight(i),
      i => this.getColumnLeft(i),
      this.setup.columnCount,
      this.scrollLeft + this.fixedColumnsTotalWidth,
      this.scrollLeft + this.innerWidth
    );
  }

  @computed
  get visibleColumnsFirstIndex() {
    return this.visibleColumnsRange.fgte;
  }

  @computed
  get visibleColumnsLastIndex() {
    return this.visibleColumnsRange.llte;
  }

  @computed
  public get fixedColumnCount(): number {
    return this.setup.fixedColumnCount;
  }

  @computed
  public get columnHeadersOffsetLeft() {
    return -this.state.scrollLeft;
  }

  @computed
  public get elmRoot(): HTMLDivElement | null {
    return this.state.elmRoot;
  }

  @computed
  public get columnCount(): number {
    return this.setup.columnCount;
  }

  @computed
  get movingColumnsTotalWidth() {
    let totWidth = 0;
    const columnCount = this.columnCount;
    for (let i = this.fixedColumnCount; i < columnCount; i++) {
      totWidth = totWidth + this.getColumnRight(i) - this.getColumnLeft(i);
    }
    return totWidth;
  }

  @computed
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

  @computed
  public get scrollLeft(): number {
    return this.state.scrollLeft;
  }

  @computed
  public get scrollTop(): number {
    return this.state.scrollTop;
  }

  @computed
  public get canvasContext(): CanvasRenderingContext2D | null {
    return this.state.canvasContext;
  }

  @computed
  public get elmScroller(): HTMLDivElement | null {
    return this.state.elmScroller;
  }

  public get onOutsideClick(): ((event: any) => void) | undefined {
    return this.state.onOutsideClick;
  }

  public get onNoCellClick(): ((event: any) => void) | undefined {
    return this.state.onNoCellClick;
  }

  public get onScroll(): ((event: any) => void) | undefined {
    return this.state.onScroll;
  }

  public get onKeyDown(): ((event: any) => void) | undefined {
    return this.state.onKeyDown;
  }
}
