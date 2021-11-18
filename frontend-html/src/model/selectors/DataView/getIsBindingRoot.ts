import {getDataView} from "./getDataView";

export function getIsBindingRoot(ctx: any) {
  return getDataView(ctx).isBindingRoot;
}
