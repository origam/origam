import { IScrollState } from "./IScrollState";
import { ICells } from "./ICells";
import { IFormField } from "./ICursor";

export interface ITable {
  isLoading: boolean;
  filterSettingsVisible: boolean;
  scrollState: IScrollState;
  cells: ICells;
  tableFormField: IFormField;
  onKeyDown?(event: any): void;
  onCellClick?(event: any, rowIdx: number, columnIdx: number): void;
  onNoCellClick?(event: any): void;
  onGridOutsideClick?(event: any): void;
  onFieldClick?(event: any): void;
  onFieldFocus?(event: any): void;
  onFieldBlur?(event: any): void;
  onFieldChange?(event: any, value: any): void;
  onFieldKeyDown?(event: any): void;
  onFieldOutsideClick?(event: any): void;
}
