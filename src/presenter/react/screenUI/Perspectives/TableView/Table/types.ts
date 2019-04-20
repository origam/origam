import { ICell, IFormField, ICells, IHeader } from "../../../../../view/Perspectives/TableView/types";
import { PubSub } from "../../../../../../utils/events";



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
  cell: ICell,
  cursor: IFormField,
  onCellClick: PubSub<any>
) => void;

export interface IScrollOffsetSource {
  scrollTop: number;
  scrollLeft: number;
}

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

export interface IGridCanvasProps {
  // How big is the canvas (CSS units)
  width: number;
  height: number;

  cells: ICells;
  cursor: IFormField;

  // For fixed columns - Which column index is the first one for this canvas.
  columnStartIndex: number;
  // For fixed columns - By how many pixel should be the content shifted to the right side.
  leftOffset: number;
  // For fixced columns - Should be the horizontal shift controlled by scroll source?
  isHorizontalScroll: boolean;

  // Drawing offset
  scrollOffsetSource: IScrollOffsetSource;

  gridDimensions: IGridDimensions;

  // Cell renderer. Should draw a cell content to ctx.
  // Ctx has its origin set to the top left corner of the cell being drawn.
  // Ctx state is saved before the renderer is called and its state restored
  // just after it ends its business.
  // Renderer is called exactly once for each visible cell.
  renderCell: IRenderCell;

  onBeforeRender?(): void;
  onAfterRender?(): void;
  onVisibleDataChanged?(
    firstVisibleColumnIndex: number,
    lastVisibleColumnIndex: number,
    firstVisibleRowIndex: number,
    lastVisibleRowIndex: number
  ): void;
}

export interface IPositionedFieldProps {
  rowIndex: number;
  columnIndex: number;
  fixedColumnsCount: number;

  scrollOffsetSource: IScrollOffsetSource;
  gridDimensions: IGridDimensions;
  worldBounds: {
    width: number;
    height: number;
    top: number;
    left: number;
    bottom: number;
    right: number;
  };
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

export interface IScrollOffsetTarget {
  setScrollOffset(event: any, scrollTop: number, scrollLeft: number): void;
}

export interface IScrolleeProps {
  width?: number | string;
  height?: number | string;
  fixedHoriz?: boolean;
  fixedVert?: boolean;
  scrollOffsetSource: IScrollOffsetSource;
}

export interface IHeaders {
  getHeader(columnIdx: number): IHeader;
}

export interface IHeaderRowProps {
  headers: IHeaders;
  gridDimensions: IGridDimensions;
  columnStartIndex: number;
  columnEndIndex: number;
}
