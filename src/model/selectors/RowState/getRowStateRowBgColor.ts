import {getRowStateById} from "./getRowStateById";

export function getRowStateRowBgColor(ctx: any, rowId: string | undefined) {
  if(rowId === undefined){
    return undefined;
  }
  const rowState = getRowStateById(ctx, rowId);
  const rowStateBackgroundColor = rowState
    ? rowState.backgroundColor
    : undefined;
  return rowStateBackgroundColor;
}
