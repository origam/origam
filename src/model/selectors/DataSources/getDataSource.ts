import { getDataView } from "../DataView/getDataView";

export function getDataSource(ctx: any) {
  return getDataView(ctx).dataSource
}