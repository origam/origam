import {getDataView} from "./getDataView";
import {IGrouper} from "model/entities/types/IGrouper";
import {isLazyLoading} from "model/selectors/isLazyLoading";

export function getGrouper(ctx: any): IGrouper {
  const serverSideGrouping = isLazyLoading(ctx)
  return serverSideGrouping 
    ? getDataView(ctx).serverSideGrouper
    : getDataView(ctx).clientSideGrouper;
}