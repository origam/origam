import { getOpenedScreens } from "./getOpenedScreens";

export function getOpenedDialogScreenItems(ctx: any) {
  return getOpenedScreens(ctx).items.filter(item => item.isDialog);
}