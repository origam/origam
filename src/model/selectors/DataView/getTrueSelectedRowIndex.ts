import {getDataView} from "./getDataView";

export function getTrueSelectedRowIndex(ctx: any) {
  return getDataView(ctx).trueSelectedRowIndex;
}