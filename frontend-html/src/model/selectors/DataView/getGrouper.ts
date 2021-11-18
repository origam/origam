import {getDataView} from "./getDataView";
import {IGrouper} from "model/entities/types/IGrouper";

export function getGrouper(ctx: any): IGrouper {
  let dataView = getDataView(ctx);
  if(!dataView){
    throw new Error("DataView is needed to determine what kind of grouper should be used");
  }
  const serverSideGrouping = dataView.isLazyLoading
  return serverSideGrouping 
    ? dataView.serverSideGrouper
    : dataView.clientSideGrouper;
}