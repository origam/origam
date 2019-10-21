import { getAction } from "./getAction";

export function getIsEnabledAction(ctx: any) {
  return getAction(ctx).isEnabled;
}
