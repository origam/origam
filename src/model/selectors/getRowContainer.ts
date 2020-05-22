import {ListRowContainer, ScrollRowContainer} from "../entities/RowsContainer";
import {getDontRequestData} from "./getDontRequestData";

export function getRowContainer(ctx: any) {
  const infiniteScrollingActive = getDontRequestData(ctx);
  return infiniteScrollingActive
    ? new ScrollRowContainer()
    : new ListRowContainer();
}