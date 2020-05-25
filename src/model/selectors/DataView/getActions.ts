import { getDataView } from "./getDataView";

export function getActions(ctx: any) {
  return getDataView(ctx).actions;
}
