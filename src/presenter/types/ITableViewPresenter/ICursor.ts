import { ITableField } from "./ITableField";

export interface IFormField {
  field: ITableField | undefined;
  rowIndex: number;
  columnIndex: number;
  isEditing: boolean;
}