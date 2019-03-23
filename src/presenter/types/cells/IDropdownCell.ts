import { IOrderByState } from "../ITableViewPresenter/IOrderByState";


export interface IHeader {
  label: string;
  orderBy: IOrderByState;
}

export interface ICell {
  isLoading: boolean;
  value: any;
}

export interface IDropdownTable {
  rowCount: number;
  columnCount: number;
  rowHeight: number;
  columnWidth: number;

  highlightPhrase: string;

  getCell(rowIdx: number, columnIdx: number): ICell;
  getHeader(columnIdx: number): IHeader;
}

export interface IDropdownCell {
  type: "DropdownCell";
  value: string;
  text: string;
  isLoading: boolean;
  dropdownTable: IDropdownTable | undefined;
}