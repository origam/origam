import {IDataView} from "../entities/types/IDataView";
import {getDontRequestData} from "./getDontRequestData";

export function isInfiniteScrollingActive(ctx: any, dataViewAttributes: any) {
  const isNonRootGridWithRootEntity = dataViewAttributes.IsRootEntity && !dataViewAttributes.IsRootGrid && getDontRequestData(ctx);
  return getDontRequestData(ctx) && dataViewAttributes.IsRootGrid && !isNonRootGridWithRootEntity;
}