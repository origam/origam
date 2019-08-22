import { getDataView } from "./getDataView";

export function getEntity(ctx: any) {
  return getDataView(ctx).entity;
}