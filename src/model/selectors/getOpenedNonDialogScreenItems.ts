import { getOpenedScreens } from "./getOpenedScreens";

export function getOpenedNonDialogScreenItems(ctx: any) {
  return getOpenedScreens(ctx).items.filter(item => !item.isDialog);
}