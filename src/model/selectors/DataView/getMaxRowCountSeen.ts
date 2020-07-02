import {getDataView} from "./getDataView";

export function getMaxRowCountSeen(ctx: any) {
  return getDataView(ctx).maxRowCountSeen;
}