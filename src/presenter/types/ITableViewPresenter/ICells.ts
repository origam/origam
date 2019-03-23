import { ICell } from "./ICell";
import { IHeader } from './IHeader';

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
