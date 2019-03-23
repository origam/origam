import { IDropdownCell } from "./IDropdownCell";
import { IBoolCell } from "./IBoolCell";
import { IDateTimeCell } from "./IDateTimeCell";
import { ITextCell } from "./ITextCell";
import { INumberCell } from "./INumberCell";

export type ICellTypeDU =
  | IBoolCell
  | IDateTimeCell
  | IDropdownCell
  | INumberCell
  | ITextCell;
