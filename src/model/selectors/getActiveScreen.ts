import { getOpenedScreens } from "./getOpenedScreens";

export function getActiveScreen(ctx: any) {
  return getOpenedScreens(ctx).activeItem
}