import { getDataSource } from "./getDataSource";

export function getDataSourceFieldByName(ctx: any, name: string) {
  return getDataSource(ctx).getFieldByName(name);
}
