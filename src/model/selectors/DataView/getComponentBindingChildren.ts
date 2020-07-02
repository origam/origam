import {getDataView} from "./getDataView";

export function getComponentBindingChildren(ctx: any) {
  return getDataView(ctx).childBindings.map(b => b.childDataView);
}
