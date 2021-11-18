import {getRowStateById} from "./getRowStateById";

export function getRowStateAllowCreate(ctx: any, rowId: string) {
  const rowState = getRowStateById(ctx, rowId);
  return rowState ? rowState.allowCreate : undefined;
}