import {getDataView} from "./getDataView";

export function getBindingToParent(ctx: any) {
  const pb = getDataView(ctx).parentBindings;
  return pb.length > 0 ? pb[0] : undefined;
}