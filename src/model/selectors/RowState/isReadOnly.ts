import { IProperty } from "../../entities/types/IProperty";
import { getRowStateAllowUpdate } from "./getRowStateAllowUpdate";

export function isReadOnly(property: IProperty, rowId: string | undefined) {
  return property!.readOnly || !getRowStateAllowUpdate(property, rowId || "", property!.id);
}