import {getDataView} from "./getDataView";

export function getBindingChildren(ctx: any) {
  return getDataView(ctx).childBindings.map(cb => cb.childDataView)
}