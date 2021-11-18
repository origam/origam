import {getOpenedScreens} from "./getOpenedScreens";

export function getOpenedScreenItems(ctx: any) {
  return getOpenedScreens(ctx).items;
}