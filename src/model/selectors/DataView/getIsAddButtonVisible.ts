import {getDataView} from "./getDataView";

export function getIsAddButtonVisible(ctx: any) {
  return getDataView(ctx).showAddButton;
}