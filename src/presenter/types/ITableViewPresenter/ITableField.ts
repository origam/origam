import { ICellTypeDU } from "../cells/ICellTypeDU";

export interface IField {
  isLoading: boolean;
  isInvalid: boolean;
  isReadOnly: boolean;
}

export type ITableField = IField & ICellTypeDU;