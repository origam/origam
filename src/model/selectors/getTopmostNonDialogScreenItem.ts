import {getOpenedNonDialogScreenItems} from "./getOpenedNonDialogScreenItems";
import {IOpenedScreen} from "model/entities/types/IOpenedScreen";

export function getTopmostOpenedNonDialogScreenItem(ctx: any): IOpenedScreen | undefined {
  const screens = [...getOpenedNonDialogScreenItems(ctx)];
  screens.sort((a, b) => b.stackPosition - a.stackPosition);
  return screens[0];
}
