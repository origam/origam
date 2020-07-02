import {getOpenedScreen} from "./getOpenedScreen";

export function getMenuItemId(ctx: any) {
  return getOpenedScreen(ctx).menuItemId;
}