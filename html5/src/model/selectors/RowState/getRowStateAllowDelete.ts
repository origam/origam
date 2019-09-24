import { getRowStateById } from "./getRowStateById";

export function getRowStateAllowDelete(ctx: any, rowId: string) {
  const rowState = getRowStateById(ctx, rowId);
  return rowState ? rowState.allowDelete : undefined;
}