import { getDataView } from "../DataView/getDataView";

export function getRowStates(ctx: any) {
  return getDataView(ctx).rowState;
}