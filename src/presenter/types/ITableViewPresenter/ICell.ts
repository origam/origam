import { ICellTypeDU } from "../cells/ICellTypeDU";

interface ITableCell {
  isLoading: boolean;
  isInvalid: boolean;
  isReadOnly: boolean;
  isCellCursor: boolean;
  isRowCursor: boolean;
  onCellClick?(event: any): void;
}

export type ICell = ICellTypeDU & ITableCell;
