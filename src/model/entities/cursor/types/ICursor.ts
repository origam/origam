import { IPropertyId } from "../../values/types/IPropertyId";
import { IRecordId } from "../../values/types/IRecordId";

export interface IExtSelRowState {
  selRowId: IRecordId | undefined;
}

export interface ICursor {
  selRowId: IRecordId | undefined;
  selColId: IPropertyId | undefined;
  selRowIdx: number | undefined;
  selColIdx: number | undefined;
  selColIdxReo: number | undefined; // Index for reordered/hidden fields
  isEditing: boolean;
  isSelected: boolean;

  selectCell(
    rowId: IRecordId | undefined,
    colId: IPropertyId | undefined
  ): void;
  selectCellByIdx(rowIdx: number | undefined, colIdx: number | undefined): void;
  selectRow(rowId: IRecordId | undefined): void;
  selectColumn(rowId: IPropertyId | undefined): void;
  selectClosestRowToId(rowId: IRecordId): void;
  selectNextRow(): void;
  selectPrevRow(): void;
  selectNextColumn(): void;
  selectPrevColumn(): void;
  selectFirstColumn(): void;

  startEditRow(rowId: IRecordId): void;
  startEditing(): void;
  finishEditing(): void;
  cancelEditing(): void;
  editCell(rowId: IRecordId, colId: IPropertyId): void;
}
