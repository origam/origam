import {getRowStateById} from "./getRowStateById";

export function getRowStateAllowUpdate(ctx: any, rowId: string, columnId: string) {
  const rowState = getRowStateById(ctx, rowId);
  const column = rowState ? rowState.columns.get(columnId) : undefined;
  const allowUpdate = column ? column.allowUpdate : undefined;
  return allowUpdate !== undefined ? allowUpdate : true;
}