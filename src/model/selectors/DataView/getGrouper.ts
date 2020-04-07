import { getDataView } from "./getDataView";
import { Grouper } from "model/entities/Grouper";

export function getGrouper(ctx: any): Grouper {
  return getDataView(ctx).grouper;
}