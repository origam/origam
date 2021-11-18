import {getDataView} from "./getDataView";

export function getSelectedRowIndex(ctx: any) {
  return getDataView(ctx).selectedRowIndex;
}