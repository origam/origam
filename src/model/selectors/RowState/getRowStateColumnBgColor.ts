import {getRowStateById} from "./getRowStateById";

export function getRowStateColumnBgColor(ctx: any, rowId: string, columnId: string) {
  const rowState = getRowStateById(ctx, rowId);
  const column = rowState ? rowState.columns.get(columnId) : undefined;
  const columnBackgroundColor = column ? column.backgroundColor : undefined;
  return columnBackgroundColor;
}