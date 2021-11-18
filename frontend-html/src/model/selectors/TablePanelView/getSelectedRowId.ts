import {getDataView} from "../DataView/getDataView";

export function getSelectedRowId(ctx: any) {
  return getDataView(ctx).selectedRowId;
}