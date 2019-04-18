import { IViewType } from "../../../../DataView/types/IViewType";
import { IToolbar, ICellTypeDU } from "../types";
import { IOrderByState } from "../../../../DataView/Ordering/types";
import { PubSub } from "../../../../utils/events";
import { IListener } from '../../../../DataView/types/IDataViewMediator';


// Toolbar plus table
export interface ITableView {
  type: IViewType.Table;
  toolbar: IToolbar | undefined;
  table: ITable;
}


// Whole table
export interface ITable {
  isLoading: boolean;
  filterSettingsVisible: boolean;
  scrollState: IScrollState;
  cells: ICells;
  cursor: IFormField;
  listenMediator(listener: IListener): () => void;
  onKeyDown?(event: any): void;
  onCellClick?(event: any, rowIdx: number, columnIdx: number): void;
  onFieldClick?(event: any): void;
  onFieldFocus?(event: any): void;
  onFieldBlur?(event: any): void;
  onFieldChange?(event: any, value: any): void;
  onFieldKeyDown?(event: any): void;
  onFieldOutsideClick?(event: any): void;
}

export interface IFormField {
  field: ITableField | undefined;
  rowIndex: number;
  columnIndex: number;
  isEditing: boolean;
}

export interface IScrollState {
  scrollTop: number;
  scrollLeft: number;
  setScrollOffset(event: any, scrollTop: number, scrollLeft: number): void;
}


// Everything about the data area (data cells, headers, content size...)
export interface ICells {
  rowCount: number;
  columnCount: number;
  fixedColumnCount: number;
  contentWidth: number;
  contentHeight: number;

  getRowTop(rowIdx: number): number;
  getRowHeight(rowIdx: number): number;
  getRowBottom(rowIdx: number): number;

  getColumnLeft(columnIdx: number): number;
  getColumnWidth(columnIdx: number): number;
  getColumnRight(columnIdx: number): number;

  getCell(rowIdx: number, columnIdx: number): ICell;
  getHeader(columnIdx: number): IHeader;
}


export interface IHeader {
  label: string;
  orderBy: IOrderByState;
  // filter: IHeaderFilter;
}


// Describes one table cellwhen not editing.
interface ITableCell {
  isLoading: boolean;
  isInvalid: boolean;
  isReadOnly: boolean;
  isCellCursor: boolean;
  isRowCursor: boolean;
}

export type ICell = ICellTypeDU & ITableCell;



// Describes the field of form when editing a cell.
export interface IField {
  isLoading: boolean;
  isInvalid: boolean;
  isReadOnly: boolean;
}

export type ITableField = IField & ICellTypeDU;