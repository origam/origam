import { Rect } from "react-measure";
import { IEventSubscriber } from "src/utils/events";

export type IRenderCell = (
  rowIndex: number,
  columnIndex: number,
  topOffset: number,
  leftOffset: number,
  columnLeft: number,
  columnWidth: number,
  columnRight: number,
  rowTop: number,
  rowHeight: number,
  rowBottom: number,
  ctx: CanvasRenderingContext2D,
  onCellClick: IEventSubscriber
) => void;

export type IRenderHeader = (columnIndex: number) => React.ReactNode;

export type IRenderCursor = (
  gridDImensions: IGridDimensions,
  gridCursorPos: IGridCursorPos,
  shouldRenderCellCursor: boolean
) => React.ReactNode;

export interface IGridCursorPos {
  selectedRowIndex: number | undefined;
  selectedColumnIndex: number | undefined;
}

export interface IGridTableProps {
  gridCursorPos: IGridCursorPos;
  renderGridCursor: IRenderCursor;

  gridDimensions: IGridDimensions;
  fixedColumnSettings: IFixedColumnSettings;
  renderHeader: IRenderHeader;
  renderCell: IRenderCell;
  editor: React.ReactNode;
  showScroller: boolean;

  onOutsideClick?: (event: any) => void;
  onKeyDown?: (event: any) => void;
  onBeforeRender?(): void;
  onAfterRender?(): void;
}

export interface IGridCanvasProps {
  // How big is the canvas (CSS units)
  width: number;
  height: number;

  // Drawing offset
  scrollOffsetSource: IScrollOffsetSource;

  gridDimensions: IGridDimensions;

  // Do not follow particular scroll direction
  fixedHoriz?: boolean;
  fixedVert?: boolean;

  // Cell renderer. Should draw a cell content to ctx.
  // Ctx has its origin set to the top left corner of the cell being drawn.
  // Ctx state is saved before the renderer is called and its state restored
  // just after it ends its business.
  // Renderer is called exactly once for each visible cell.
  renderCell: IRenderCell;

  onBeforeRender?(): void;
  onAfterRender?(): void;
}

export interface IScrollOffsetSource {
  scrollTop: number;
  scrollLeft: number;
}

export interface IScrollOffsetTarget {
  setScrollOffset(scrollTop: number, scrollLeft: number): void;
}

export type IScrollOffset = IScrollOffsetSource & IScrollOffsetTarget;

export interface IGridDimensions {
  rowCount: number;
  columnCount: number;
  contentWidth: number;
  contentHeight: number;
  getColumnLeft(columnIndex: number): number;
  getColumnWidth(columnIndex: number): number;
  getColumnRight(columnIndex: number): number;
  getRowTop(rowIndex: number): number;
  getRowHeight(rowIndex: number): number;
  getRowBottom(rowIndex: number): number;
}

export interface IScrolleeProps {
  width?: number | string;
  height?: number | string;
  fixedHoriz?: boolean;
  fixedVert?: boolean;
  scrollOffsetSource: IScrollOffsetSource;
}

export interface IScrollerProps {
  width: number | string;
  height: number | string;
  isVisible: boolean;
  contentWidth: number;
  contentHeight: number;
  scrollOffsetTarget: IScrollOffsetTarget;
  onClick?: (event: any, contentLeft: number, contentTop: number) => void;
  onOutsideClick?: (event: any) => void;
  onKeyDown?: (event: any) => void;
}

export interface IGridTableLayoutProps {
  width: number | string;
  height: number | string;
  fixedColumnsTotalWidth: number;

  fixedColumnsHeaders: React.ReactNode;
  movingColumnsHeaders: React.ReactNode;
  fixedColumnsCanvas: React.ReactNode;
  movingColumnsCanvas: React.ReactNode;
  fixedColumnsCursor: React.ReactNode;
  movingColumnsCursor: React.ReactNode;
  scroller: React.ReactNode;
  editor: React.ReactNode;
}

export interface IGridCursorProps {
  pollCellRect: boolean;
  showCellCursor: boolean;
  gridDimensions: IGridDimensions;
  gridCursorPos: IGridCursorPos;
  renderCellContent: (cursorRect: Rect) => React.ReactNode;
}

export interface IFixedColumnSettings {
  fixedColumnCount: number;
}
