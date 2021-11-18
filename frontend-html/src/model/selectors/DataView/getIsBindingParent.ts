import {getDataView} from "./getDataView";

export function getIsBindingParent(ctx: any) {
  return getDataView(ctx).isBindingParent;
}
