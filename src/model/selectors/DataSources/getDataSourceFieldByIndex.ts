import {getDataSource} from "./getDataSource";

export function getDataSourceFieldByIndex(ctx: any, index: number) {
  return getDataSource(ctx).getFieldByIndex(index);
}
