import { getOpenedScreens } from "./getOpenedScreens";

export function getOpenedDialogItems(ctx: any) {
  return getOpenedScreens(ctx).dialogItems;
}
