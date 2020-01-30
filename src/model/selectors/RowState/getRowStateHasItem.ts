import { getRowStates } from "./getRowStates";

export function getRowStateHasItem(ctx: any, key: string) {
  return getRowStates(ctx).hasValue(key);
}