import {getRowStateById} from "./getRowStateById";

export function getRowStateForegroundColor(ctx: any, rowId: string) {
  const rowState = getRowStateById(ctx, rowId);
  const rowStateForegroundColor = rowState ? rowState.foregroundColor : undefined;
  if(rowStateForegroundColor) {
    return rowStateForegroundColor;
  }
  return rowState ? rowState.foregroundColor : undefined;
}