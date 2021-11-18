import {getOrderingConfiguration} from "./getOrderingConfiguration";

export function getPropertyOrdering(ctx: any, column: string) {
  return getOrderingConfiguration(ctx).getOrdering(column);
}
