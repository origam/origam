import { getDataSource } from "../DataSources/getDataSource";

export function getRowStates(ctx: any) {
  return getDataSource(ctx).rowState;
}