import { getDataView } from "./getDataView";

export function getRowState(ctx: any) {
  return getDataView(ctx).rowState;
}