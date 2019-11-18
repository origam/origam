import { getDataView } from "./getDataView";

export function getVisibleRowCount(ctx: any) {
  return getDataView(ctx).visibleRowCount;
}