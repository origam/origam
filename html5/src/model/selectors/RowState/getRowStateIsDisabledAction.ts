import { getRowStateById } from "./getRowStateById";

export function getRowStateIsDisableAction(ctx: any, rowId: string, actionId: string) {
  const rowState = getRowStateById(ctx, rowId);
  return rowState ? rowState.disabledActions.has(actionId) : false;
}