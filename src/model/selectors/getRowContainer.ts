import {ListRowContainer, ScrollRowContainer} from "../entities/RowsContainer";
import {isInfiniteScrollingActive} from "./isInfiniteScrollingActive";
import {OrderingConfiguration} from "../entities/OrderingConfiguration";

export function getRowContainer(ctx: any, dataViewAttributes: any, orderingConfiguration: OrderingConfiguration) {
  return isInfiniteScrollingActive(ctx, dataViewAttributes)
    ? new ScrollRowContainer()
    : new ListRowContainer(orderingConfiguration);
}