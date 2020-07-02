import {getOpenedScreen} from "./getOpenedScreen";

export function getMenuItemType(ctx: any) {
  return getOpenedScreen(ctx).menuItemType;
}