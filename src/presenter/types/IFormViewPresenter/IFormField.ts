import { ICellTypeDU } from "../cells/ICellTypeDU";

interface IField {
  isLoading: boolean;
  isInvalid: boolean;
  isReadOnly: boolean;
}

export type IFormField = ICellTypeDU & IField;


