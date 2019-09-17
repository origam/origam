import { getOpenedDialogScreens } from "./getOpenedDialogScreens";

export function getOpenedDialogItems(ctx: any) {
  return getOpenedDialogScreens(ctx).items;
}
