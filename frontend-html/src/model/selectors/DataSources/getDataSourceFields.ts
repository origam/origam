import {getDataSource} from "./getDataSource";

export function getDataSourceFields(ctx: any) {
  return getDataSource(ctx).fields
}