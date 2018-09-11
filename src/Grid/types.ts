export interface IGridProps {
  view: IGridView;
  width: number;
  height: number;
  overlayElements: React.Component | React.Component[] | null;
}

export interface IGridView {
  componentDidMount(
    props: IGridProps,
    component: React.Component<IGridProps>
  ): void;

  componentDidUpdate(
    prevProps: IGridProps,
    props: IGridProps,
    component: React.Component<IGridProps>
  ): void;

  componentWillUnmount(): void;

  canvasProps: {
    width: number;
    height: number;
    style: {
      width: number;
      height: number;
    };
  };

  contentWidth: number;
  contentHeight: number;

  handleGridScroll(event: any): void;
  handleGridKeyDown(event: any): void;
  handleGridClick(event: any): void;
  refRoot(element: HTMLDivElement): void;
  refScroller(element: HTMLDivElement): void;
  refCanvas(element: HTMLCanvasElement): void;
}

export interface IGridSelectors {
  width: number;
  height: number;
  innerWidth: number;
  innerHeight: number;
  contentWidth: number;
  contentHeight: number;
  visibleRowsFirstIndex: number;
  visibleRowsLastIndex: number;
  visibleColumnsFirstIndex: number;
  visibleColumnsLastIndex: number;
  fixedColumnCount: number;
  columnHeadersOffsetLeft: number;
  elmRoot: React.Component;
  columnCount: number;
  fixedColumnsTotalWidth: number;

  fixedShiftedColumnDimensions(
    left: number,
    width: number,
    columnIndex: number
  ): { left: number; width: number };
  cellRenderer: () => {};
  scrollLeft: number;
  scrollTop: number;
  canvasContext: CanvasRenderingContext2D;
  elmScroller: number;

  getRowTop(rowIndex: number): number;
  getRowBottom(rowIndex: number): number;
  getRowHeight(rowIndex: number): number;
  getColumnLeft(columnIndex: number): number;
  getColumnRight(columIndex: number): number;
  getColumnWidth(columnIndex: number): number;
  getColumnId(columnIndex: number): number;

  onOutsideClick: ((event: any) => void) | undefined;
  onScroll: ((event: any) => void) | undefined;
}

export interface IGridState {
  width: number;
  height: number;

  setComponent(component: React.Component): void;
  setSize(width: number, height: number): void;
  setScroll(scrollTop: number, scrollLeft: number): void;
  setRefRoot(element: HTMLDivElement): void;
  setRefScroller(element: HTMLDivElement): void;
  setRefCanvas(element: HTMLCanvasElement): void;
  setCanvasContext(context: CanvasRenderingContext2D | null): void;
}

export interface IGridActions {
  handleResize(width: number, height: number): void;
  handleScroll(event: any): void;
  handleKeyDown(event: any): void;
  refRoot(element: HTMLDivElement): void;
  refScroller(element: HTMLDivElement): void;
  refCanvas(element: HTMLCanvasElement): void;
}

export interface IGridSetup {
  columnCount: number;
  fixedColumnCount: number;
  rowCount: number;
  isScrollingEnabled: boolean;
  getCellTop(cellIndex: number): number;
  getCellLeft(cellIndex: number): number;
  getCellBottom(cellIndex: number): number;
  getCellRight(cellIndex: number): number;
  getCellValue(rowIndex: number, columnIndex: number): string;
  getColumnLabel(columnIndex: number): string;
  getRowTop(rowIndex: number): number;
  getRowBottom(rowIndex: number): number;
  getRowHeight(rowIndex: number): number;
  getColumnLeft(columnIndex: number): number;
  getColumnRight(columIndex: number): number;
  getColumnWidth(columnIndex: number): number;

}

export interface IGridTopology {
  getUpRowId(rowId: string): string;
  getDownRowId(rowId: string): string;
  getLeftColumnId(columnId: string): string;
  getRightColumnId(columnId: string): string;
  getColumnIdByIndex(columnIndex: number): string;
  getRowIdByIndex(rowIndex: number): string
  getColumnIndexById(columnId: string): number;
  getRowIndexById(rowId: string): number;
}