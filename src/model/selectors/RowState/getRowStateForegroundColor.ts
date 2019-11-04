import { getRowStateById } from "./getRowStateById";

export function getRowStateForegroundColor(ctx: any, rowId: string, columnId: string) {
  const rowState = getRowStateById(ctx, rowId);
  const rowStateForegroundColor = rowState ? rowState.foregroundColor : undefined;
  if(rowStateForegroundColor) {
    return rowStateForegroundColor;
  }
  const column = rowState ? rowState.columns.get(columnId) : undefined;
  const columnForegroundColor = column ? column.foregroundColor : undefined;
  return columnForegroundColor;
}