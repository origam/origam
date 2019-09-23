import { getRowStateById } from "./getRowStateById";

export function getRowStateAllowRead(ctx: any, rowId: string, columnId: string) {
  const rowState = getRowStateById(ctx, rowId);
  const column = rowState ? rowState.columns.get(columnId) : undefined;
  const allowRead = column ? column.allowRead : undefined;
  return allowRead;
}