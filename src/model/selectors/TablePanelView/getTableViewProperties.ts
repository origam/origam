import { getDataView } from "../DataView/getDataView";

export function getTableViewProperties(ctx: any) {
  return getDataView(ctx).tableViewProperties;
}