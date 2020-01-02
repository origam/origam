import { getAction } from "./getAction";

export function getActionCaption(ctx: any) {
  return getAction(ctx).caption;
}
