import { ICellTypeDU } from "../cells/ICellTypeDU";

interface ITableCell {
  isLoading: boolean;
  isInvalid: boolean;
  isReadOnly: boolean;
  isCellCursor: boolean;
  isRowCursor: boolean;
}

export type ICell = ICellTypeDU & ITableCell;
