import { getDataView } from "./getDataView";
import { getDataSource } from "../DataSources/getDataSource";

export function getEntity(ctx: any) {
  return getDataSource(ctx).entity;
}
