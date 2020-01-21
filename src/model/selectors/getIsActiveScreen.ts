import { getOpenedScreen } from "./getOpenedScreen";

export function getIsActiveScreen(ctx: any) {
  return getOpenedScreen(ctx).isActive;
}