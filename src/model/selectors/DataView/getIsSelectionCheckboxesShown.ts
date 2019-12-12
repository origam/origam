import { getDataView } from "./getDataView";

export function getIsSelectionCheckboxesShown(ctx: any) {
  return getDataView(ctx).showSelectionCheckboxes;
}
