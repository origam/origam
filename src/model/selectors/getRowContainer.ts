import {ListRowContainer, ScrollRowContainer} from "../entities/RowsContainer";
import {isInfiniteScrollingActive} from "./isInfiniteScrollingActive";

export function getRowContainer(ctx: any, dataViewAttributes: any) {
  return isInfiniteScrollingActive(ctx, dataViewAttributes)
    ? new ScrollRowContainer()
    : new ListRowContainer();
}