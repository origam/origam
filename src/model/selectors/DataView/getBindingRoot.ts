import {getDataView} from "./getDataView";

export function getBindingRoot(ctx: any) {
  return getDataView(ctx).bindingRoot;
}