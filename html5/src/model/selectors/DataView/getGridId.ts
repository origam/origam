import { getDataView } from "./getDataView";

export function getGridId(ctx: any): string {
  return getDataView(ctx).modelInstanceId;
}