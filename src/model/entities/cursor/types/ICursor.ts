import { IPropertyId } from "../../values/types/IPropertyId";
import { IRecordId } from "../../values/types/IRecordId";

export interface IEditCtx {}

export interface ISelectCtx {}

export interface ICursor {
  selRowId: IRecordId | undefined;
  selColId: IPropertyId | undefined;
  selRowIdx: number | undefined;
  selColumnIdx: number | undefined;
  isEditing: boolean;
  isSelected: boolean;

  selectCell(
    rowId: IRecordId | undefined,
    colId: IPropertyId | undefined
  ): void;
  selectRow(rowId: IRecordId | undefined): void;
  selectColumn(rowId: IPropertyId | undefined): void;
  selectClosestRowToId( rowId: IRecordId): void;
  selectNextRow(): void;
  selectPrevRow(): void;
  selectNextColumn(): void;
  selectPrevColumn(): void;

  startEditRow(rowId: IRecordId): void;
  editCell(
    rowId: IRecordId,
    colId: IPropertyId
  ): void;
}
