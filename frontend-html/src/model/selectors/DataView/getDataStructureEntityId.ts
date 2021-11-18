import {getDataSource} from "../DataSources/getDataSource";

export function getDataStructureEntityId(ctx: any): string {
  return getDataSource(ctx).dataStructureEntityId;
}
