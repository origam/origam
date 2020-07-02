import {getDataView} from "./getDataView";

export function getIsCopyButtonVisible(ctx: any) {
  return getDataView(ctx).showAddButton;
}