import { getRowStateById } from "./getRowStateById";

export function getRowStateBackgroundColor(ctx: any, rowId: string, columnId: string) {
  const rowState = getRowStateById(ctx, rowId);
  const rowStateBackgroundColor = rowState ? rowState.backgroundColor : undefined;
  if(rowStateBackgroundColor) {
    return rowStateBackgroundColor;
  }
  const column = rowState ? rowState.columns.get(columnId) : undefined;
  const columnBackgroundColor = column ? column.backgroundColor : undefined;
  return columnBackgroundColor;
}