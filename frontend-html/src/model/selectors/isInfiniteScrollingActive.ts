import {isLazyLoading} from "./isLazyLoading";
import {getDataView} from "./DataView/getDataView";

export function isInfiniteScrollingActive(ctx: any, dataViewAttributes?: any) {

  const isRootGrid = dataViewAttributes
    ? dataViewAttributes.IsRootGrid === "true"
    : getDataView(ctx).isRootGrid

    return isLazyLoading(ctx) && isRootGrid;
}