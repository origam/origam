import { getDataView } from "./getDataView";

export function getIsDelButtonVisible(ctx: any) {
  return getDataView(ctx).showDeleteButton;
}
