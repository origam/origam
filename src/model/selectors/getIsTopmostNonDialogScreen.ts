import {getOpenedScreen} from "./getOpenedScreen";
import {getTopmostOpenedNonDialogScreenItem} from "./getTopmostNonDialogScreenItem";

export function getIsTopmostNonDialogScreen(ctx: any) {
  const screen = getOpenedScreen(ctx);
  const topmostNonDialogScreen = getTopmostOpenedNonDialogScreenItem(ctx);
  if (screen && topmostNonDialogScreen) {
    return (
      screen.menuItemId === topmostNonDialogScreen.menuItemId &&
      screen.order === topmostNonDialogScreen.order
    );
  }
  return false;
}
