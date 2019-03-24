import { ILookupId } from "../../values/types/ILookupId";
import { IDropDownColumn } from "./IDropDownColumn";

export interface ILookup {
  id: ILookupId;
  dropDownColumns: IDropDownColumn[];
}