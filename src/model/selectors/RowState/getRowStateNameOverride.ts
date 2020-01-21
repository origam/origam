import { getRowStateById } from "./getRowStateById";

export function getRowStateDynamicLabel(ctx: any, rowId: string, columnId: string) {
  const rowState = getRowStateById(ctx, rowId);
  const column = rowState ? rowState.columns.get(columnId) : undefined;
  const dynamicLabel = column ? column.dynamicLabel : undefined;
  return dynamicLabel;
}