import { ICellValue, IFieldId, IRecordId } from "../DataTable/types";

export type ICellRenderer = (args: ICellRendererArgs) => void;

export interface IGridProps {
  view: IGridView;
  gridSetup: IGridSetup;
  gridTopology: IGridTopology;
  width: number;
  height: number;
  overlayElements: React.ReactNode | React.ReactNode[] | null;
  cellRenderer: ICellRenderer;
  onScroll?: ((event: any) => void) | undefined;
  onOutsideClick?: ((event: any) => void) | undefined;
  onNoCellClick?: ((event: any) => void) | undefined;
  onKeyDown?: ((event: any) => void) | undefined;
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
  fixedColumnCount: number;
  movingColumnsTotalWidth: number;
  columnHeadersOffsetLeft: number;
  columnCount: number;
  isScrollingEnabled: boolean;
  getColumnId(columnIndex: number): IFieldId | undefined;
  getColumnLeft(columnIndex: number): number;
  getColumnRight(columIndex: number): number;

  handleGridScroll(event: any): void;
  handleGridKeyDown(event: any): void;
  handleGridClick(event: any): void;
  refRoot(element: HTMLDivElement): void;
  refScroller(element: HTMLDivElement): void;
  refCanvas(element: HTMLCanvasElement): void;
}

type IClickHandler = (
  event: any,
  cellRect: ICellRect,
  cellInfo: ICellInfo
) => void;
type IClickHandlerSubscribe = (handler: IClickHandler) => void;

export interface ICellRendererArgs {
  columnIndex: number;
  rowIndex: number;
  cellDimensions: {
    width: number;
    height: number;
  };
  ctx: CanvasRenderingContext2D;
  events: {
    onClick: IClickHandlerSubscribe;
  };
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
  rowCount: number;
  fixedColumnCount: number;
  columnHeadersOffsetLeft: number;
  isScrollingEnabled: boolean;
  elmRoot: HTMLDivElement | null;
  columnCount: number;
  fixedColumnsTotalWidth: number;
  movingColumnsTotalWidth: number;

  fixedShiftedColumnDimensions(
    left: number,
    width: number,
    columnIndex: number
  ): { left: number; width: number };
  cellRenderer(args: ICellRendererArgs): void;
  scrollLeft: number;
  scrollTop: number;
  canvasContext: CanvasRenderingContext2D | null;
  elmScroller: HTMLDivElement | null;

  getRowTop(rowIndex: number): number;
  getRowBottom(rowIndex: number): number;
  getRowHeight(rowIndex: number): number;
  getColumnLeft(columnIndex: number): number;
  getColumnRight(columIndex: number): number;
  getColumnWidth(columnIndex: number): number;
  getColumnId(columnIndex: number): IFieldId | undefined;

  onOutsideClick: ((event: any) => void) | undefined;
  onNoCellClick: ((event: any) => void) | undefined;
  onScroll: ((event: any) => void) | undefined;
  onKeyDown: ((event: any) => void) | undefined;
}

export interface IGridState {
  width: number;
  height: number;
  scrollTop: number;
  scrollLeft: number;
  component: React.Component | null;
  elmRoot: HTMLDivElement | null;
  elmScroller: HTMLDivElement | null;
  elmCanvas: HTMLCanvasElement | null;
  canvasContext: CanvasRenderingContext2D | null;

  cellRenderer: ICellRenderer;
  onOutsideClick: ((event: any) => void) | undefined;
  onNoCellClick: ((event: any) => void) | undefined;
  onScroll: ((event: any) => void) | undefined;
  onKeyDown: ((event: any) => void) | undefined;

  setSize(width: number, height: number): void;
  setScroll(scrollTop: number, scrollLeft: number): void;
  setScrollTop(scrollTop: number): void;
  setScrollLeft(scrollLeft: number): void;
  setRefRoot(element: HTMLDivElement): void;
  setRefScroller(element: HTMLDivElement): void;
  setRefCanvas(element: HTMLCanvasElement): void;
  setCanvasContext(context: CanvasRenderingContext2D | null): void;
  setCellRenderer(cellRenderer: ICellRenderer): void;  
  setOnOutsideClick(handler: (((event: any) => void)) | undefined): void;
  setOnNoCellClick(handler: (((event: any) => void)) | undefined): void;
  setOnScroll(handler: (((event: any) => void)) | undefined): void;
  setOnKeyDown(handler: ((event: any) => void) | undefined): void;
}

export interface IGridActions {
  handleResize(width: number, height: number): void;
  handleScroll(event: any): void;
  handleKeyDown(event: any): void;
  handleGridClick(event: any): void;
  componentDidMount(props: IGridProps): void;
  componentDidUpdate(prevProps: IGridProps, props: IGridProps): void;
  componentWillUnmount(): void;
  refRoot(element: HTMLDivElement): void;
  refScroller(element: HTMLDivElement): void;
  refCanvas(element: HTMLCanvasElement): void;

  performScrollTo({
    scrollTop,
    scrollLeft
  }: {
    scrollTop?: number;
    scrollLeft?: number;
  }): void;
  performIncrementScroll({
    scrollTop,
    scrollLeft
  }: {
    scrollTop?: number;
    scrollLeft?: number;
  }): void;
}

export interface IGridSetup {
  columnCount: number;
  fixedColumnCount: number;
  rowCount: number;
  isScrollingEnabled: boolean;
  isFixedColumn(columnIndex: number): boolean;
  getCellTop(cellIndex: number): number;
  getCellLeft(cellIndex: number): number;
  getCellBottom(cellIndex: number): number;
  getCellRight(cellIndex: number): number;
  getCellValue(rowIndex: number, columnIndex: number): ICellValue | undefined;
  getColumnLabel(columnIndex: number): string;
  getRowTop(rowIndex: number): number;
  getRowBottom(rowIndex: number): number;
  getRowHeight(rowIndex: number): number;
  getColumnLeft(columnIndex: number): number;
  getColumnRight(columIndex: number): number;
  getColumnWidth(columnIndex: number): number;
  onRowsRendered(rowIndexStart: number, rowIndexEnd: number): void;
}

export interface IGridTopology {
  getUpRowId(rowId: IRecordId): IRecordId | undefined;
  getDownRowId(rowId: IRecordId): IRecordId | undefined;
  getLeftColumnId(columnId: IFieldId): IFieldId | undefined;
  getRightColumnId(columnId: IFieldId): IFieldId | undefined;
  getColumnIdByIndex(columnIndex: number): IFieldId | undefined;
  getRowIdByIndex(rowIndex: number): IRecordId | undefined;
  getColumnIndexById(columnId: IFieldId): number;
  getRowIndexById(rowId: IRecordId): number;
}

export interface ICellRect {
  left: number;
  top: number;
  width: number;
  height: number;
  right: number;
  bottom: number;
}

export interface ICellInfo {
  columnIndex: number;
  rowIndex: number;
}

export interface IClickSubscription {
  cellRect: ICellRect;
  cellRealRect: ICellRect;
  cellInfo: ICellInfo;
  handler(event: any, cellRect: ICellRect, cellInfo: ICellInfo): void;
}

export type IColumnHeaderRenderer = ({columnIndex}: {columnIndex: number}) => React.ReactNode;

export interface IGridCursorView {
  fixedRowCursorDisplayed: boolean;
  fixedRowCursorStyle: { [key: string]: string | number | undefined };
  fixedCellCursorDisplayed: boolean;
  fixedCellCursorStyle: { [key: string]: string | number | undefined };
  movingRowCursorDisplayed: boolean;
  movingRowCursorStyle: { [key: string]: string | number | undefined };
  movingCellCursorDisplayed: boolean;
  movingCellCursorStyle: { [key: string]: string | number | undefined };
  selectedRowId: string | undefined;
  selectedColumnId: string | undefined;
  editingRowId: string | undefined;
  editingColumnId: string | undefined;
  isCellSelected: boolean;
  isCellEditing: boolean;
  editingCellValue: ICellValue | undefined;
}

export interface IGridInteractionSelectors {
  selectedRowId: IRecordId | undefined;
  selectedColumnId: IFieldId | undefined;
  editingRowId: IRecordId | undefined;
  editingColumnId: IFieldId | undefined;
  isCellSelected: boolean;
  isCellEditing: boolean;

  getLeftColumnId(columnId: IFieldId): IFieldId | undefined;
  getRightColumnId(columnId: IFieldId): IFieldId | undefined;
  getUpRowId(columnId: IRecordId): IRecordId | undefined;
  getDownRowId(columnId: IRecordId): IRecordId | undefined;
}

export interface IGridInteractionState {
  selectedRowId: string | undefined;
  selectedColumnId: string | undefined;
  editingRowId: string | undefined;
  editingColumnId: string | undefined;

  setEditing(rowId: string | undefined, columnId: string | undefined): void;
  setSelected(rowId: string | undefined, columnId: string | undefined): void;
  setSelectedColumn(columnId: string | undefined): void;
  setSelectedRow(rowId: string | undefined): void;
}

export interface IGridInteractionActions {
  select(rowId: string, columnId: string): void;
}

export type T$1 = [number];
export type T$2 = [number, number];
export type T$3 = [number, number, number];
export type T$4 = [number, number, number, number];
export type T$5 = [number, number, number, number, number];
export type T$6 = [number, number, number, number, number, number];
