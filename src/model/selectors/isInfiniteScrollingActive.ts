import {IDataView} from "../entities/types/IDataView";
import {getDontRequestData} from "./getDontRequestData";

export function isInfiniteScrollingActive(ctx: any, dataViewAttributes: any) {
  return getDontRequestData(ctx) && dataViewAttributes.IsRootGrid; // !(dataViewAttributes.isRootEntity &&
}