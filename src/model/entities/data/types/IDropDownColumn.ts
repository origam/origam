import { IDropDownColumnId } from "../../values/types/IDropDownColumnId";

export interface IDropDownColumn {
  id: IDropDownColumnId;
  name: string;
  entity: string;
  column: string;
  index: number; // ?
}