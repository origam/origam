import { getDataView } from "./getDataView";
import { IGrouper } from "model/entities/types/IGrouper";
import { getDontRequestData } from "../getDontRequestData";

export function getGrouper(ctx: any): IGrouper {
  const serverSideGrouping = getDontRequestData(ctx)
  return serverSideGrouping 
    ? getDataView(ctx).serverSideGrouper
    : getDataView(ctx).clientSideGrouper;
}