import {isInfiniteScrollingActive} from "./isInfiniteScrollingActive";
import {IOrderingConfiguration} from "../entities/types/IOrderingConfiguration";
import {IFilterConfiguration} from "../entities/types/IFilterConfiguration";
import {ScrollRowContainer} from "../entities/ScrollRowContainer";
import {ListRowContainer} from "../entities/ListRowContainer";

export function getRowContainer(ctx: any, dataViewAttributes: any,
                                orderingConfiguration: IOrderingConfiguration,
                                filterConfiguration: IFilterConfiguration,
                                rowIdGetter: (row: any[]) => string) {
  return isInfiniteScrollingActive(ctx, dataViewAttributes)
    ? new ScrollRowContainer(rowIdGetter)
    : new ListRowContainer(orderingConfiguration, filterConfiguration, rowIdGetter);
}