import {getDataSourceFieldByName} from "./getDataSourceFieldByName";

export function getDataSourceFieldIndexByName(ctx: any, name: string) {
  return getDataSourceFieldByName(ctx, name)!.index;
}
