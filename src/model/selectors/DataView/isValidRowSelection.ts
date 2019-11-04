import { getDataView } from "./getDataView";

export function isValidRowSelection(ctx: any) {
  return getDataView(ctx).isValidRowSelection;
}