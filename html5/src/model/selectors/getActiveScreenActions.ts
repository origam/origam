import { getOpenedScreens } from "./getOpenedScreens";

export function getActiveScreenActions(ctx: any) {
  return getOpenedScreens(ctx).activeScreenActions;
}