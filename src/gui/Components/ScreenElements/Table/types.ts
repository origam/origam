import { PubSub } from "../../../../utils/events";
import { IHeaderContainer } from "gui/Workbench/ScreenArea/TableView/TableView";
import { ITableRow} from "./TableRendering/types";
import {BoundingRect} from "react-measure";

export enum IOrderByDirection {
  NONE = "NONE",
  ASC = "ASC",
  DESC = "DESC"
}

export type ICellType = "Text" | "ComboBox" | "Date" | "Number" | "CheckBox" | "TagInput" | "Checklist"; 

export interface ITableProps {
  gridDimensions: IGridDimensions;
  scrollState: IScrollState;

  tableRows:  ITableRow[]
  editingRowIndex?: number;
  editingColumnIndex?: number;
  isEditorMounted: boolean;
  fixedColumnCount: number;
  isLoading?: boolean;

  headerContainers: IHeaderContainer[];

  renderEditor?: IRenderEditor;

  renderCell?: IRenderCell;

  listenForScrollToCell?: IListenForScrollToCell;
  onOutsideTableClick?(event: any): void;
  onNoCellClick?(event: any): void;
  onKeyDown?(event: any): void;
  refCanvasMovingComponent?(elm: IGridCanvas | null): void;
  onContentBoundsChanged(bounds: BoundingRect): void;
}

export type IRenderCell = (args: IRenderCellArgs) => void;

export type IRenderCellArgs = {
  rowIndex: number;
  columnIndex: number;
  topOffset: number;
  leftOffset: number;
  columnLeft: number;
  columnWidth: number;
  columnRight: number;
  rowTop: number;
  rowHeight: number;
  rowBottom: number;
  ctx: CanvasRenderingContext2D;
  onCellClick: PubSub<any>;
};

export type IRenderHeader = (args: {
  columnIndex: number;
  columnWidth: number;
}) => React.ReactNode;

export type IRenderEditor = () => React.ReactNode;

export interface IGridDimensions {
  rowCount: number;
  columnCount: number;
  contentWidth: number;
  contentHeight: number;
  getColumnLeft(dataColumnIndex: number): number;
  getColumnRight(dataColumnIndex: number): number;
  getRowTop(rowIndex: number): number;
  getRowHeight(rowIndex: number): number;
  getRowBottom(rowIndex: number): number;
  columnWidths: Map<string, number>;
  displayedColumnDimensionsCom: {left: number, width: number, right: number}[]
}

export type IListenForScrollToCell = (
  cb: (rowIdx: number, colIdx: number) => void
) => () => void;

export interface IScrollState
  extends IScrollOffsetSource,
    IScrollOffsetTarget {}

export interface IScrollOffsetSource {
  scrollTop: number;
  scrollLeft: number;
}

export interface IScrollOffsetTarget {
  setScrollOffset(event: any, scrollTop: number, scrollLeft: number): void;
}

export interface IGridCanvas {
  firstVisibleRowIndex: number;
  lastVisibleRowIndex: number;
}

export interface IGridCanvasProps {
  // How big is the canvas (CSS units)
  width: number;
  height: number;

  contentWidth: number;
  contentHeight: number;

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
  onNoCellClick?(event: any): void;
  
}

export interface IPositionedFieldProps {
  rowIndex: number;
  columnIndex: number;
  fixedColumnsCount: number;

  scrollOffsetSource: IScrollOffsetSource;
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
  scrollingDisabled: boolean;
  // scrollOffsetTarget: IScrollOffsetTarget;
  onScroll: (event: any, scrollLeft: number, scrollTop: number) => void;
  onClick?: (event: any, contentLeft: number, contentTop: number) => void;
  onOutsideClick?: (event: any) => void;
  onKeyDown?: (event: any) => void;
}

export interface IScrolleeProps {
  width?: number | string;
  height?: number | string;
  fixedHoriz?: boolean;
  fixedVert?: boolean;
  zIndex?: number | undefined;
  scrollOffsetSource: IScrollOffsetSource;
}

export interface IHeaderRowProps {
  zIndex?: number | undefined;
  headerElements: JSX.Element[];
}

export interface IRenderedCell {
  isCellCursor: boolean;
  isRowCursor: boolean;
  isColumnOrderChangeSource: boolean;
  isColumnOrderChangeTarget: boolean;
  isLoading: boolean;
  isInvalid: boolean;
  isHidden: boolean;
  isPassword?: boolean;
  isLink: boolean;
  formatterPattern: string;
  type: ICellType;
  value: any;
  text: any;
  foregroundColor?: string;
  backgroundColor?: string;
}
