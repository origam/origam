import {getOpenedScreen} from "./getOpenedScreen";

export function getIsDialog(ctx: any) {
  return getOpenedScreen(ctx).isDialog;
}
