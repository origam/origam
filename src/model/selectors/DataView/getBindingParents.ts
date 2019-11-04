import { getDataView } from "./getDataView";

export function getBindingParents(ctx: any) {
  return getDataView(ctx).parentBindings.map(pb => pb.parentDataView)
}