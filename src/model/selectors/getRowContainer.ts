import {ListRowContainer} from "../entities/RowsContainer";
import {isInfiniteScrollingActive} from "./isInfiniteScrollingActive";
import {IOrderingConfiguration} from "../entities/types/IOrderingConfiguration";
import {IFilterConfiguration} from "../entities/types/IFilterConfiguration";
import {ScrollRowContainer} from "../entities/ScrollRowContainer";

export function getRowContainer(ctx: any, dataViewAttributes: any,
                                orderingConfiguration: IOrderingConfiguration,
                                filterConfiguration: IFilterConfiguration,
                                rowIdGetter: (row: any[]) => string) {
  return isInfiniteScrollingActive(ctx, dataViewAttributes)
    ? new ScrollRowContainer(rowIdGetter)
    : new ListRowContainer(orderingConfiguration, filterConfiguration);
}