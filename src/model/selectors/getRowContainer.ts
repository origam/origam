import {ListRowContainer, ScrollRowContainer} from "../entities/RowsContainer";
import {isInfiniteScrollingActive} from "./isInfiniteScrollingActive";
import {OrderingConfiguration} from "../entities/OrderingConfiguration";
import {FilterConfiguration} from "../entities/FilterConfiguration";

export function getRowContainer(ctx: any, dataViewAttributes: any,
                                orderingConfiguration: OrderingConfiguration, filterConfiguration: FilterConfiguration) {
  return isInfiniteScrollingActive(ctx, dataViewAttributes)
    ? new ScrollRowContainer()
    : new ListRowContainer(orderingConfiguration, filterConfiguration);
}