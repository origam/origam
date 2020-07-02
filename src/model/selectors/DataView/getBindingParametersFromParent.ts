import {getDataView} from "./getDataView";

export function getBindingParametersFromParent(ctx: any) {
  // debugger
  return getDataView(ctx).bindingParametersFromParent;
}