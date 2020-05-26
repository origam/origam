import {ListRowContainer, ScrollRowContainer} from "../entities/RowsContainer";
import {getDataView} from "./DataView/getDataView";
import {isInfiniteScrollingActive} from "./isInfiniteScrollingActive";
import {IDataView} from "../entities/types/IDataView";
import {IFormScreenLifecycle02} from "../entities/types/IFormScreenLifecycle";

export function getRowContainer(ctx: any, dataViewAttributes: any) {
  // const dataView = getDataView(ctx);
  return isInfiniteScrollingActive(ctx, dataViewAttributes)
    ? new ScrollRowContainer()
    : new ListRowContainer();
}