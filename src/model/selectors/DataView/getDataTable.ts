import {getDataView} from "./getDataView";

export function getDataTable(ctx: any) {
  return getDataView(ctx).dataTable;
}