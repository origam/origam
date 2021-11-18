import {getDataTable} from "../DataView/getDataTable";

export function getRowCount(ctx: any) {
  return getDataTable(ctx).rows.length;
}
