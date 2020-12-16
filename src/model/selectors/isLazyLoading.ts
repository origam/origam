import { IMainMenuItemType } from "model/entities/types/IMainMenu";
import {getOpenedScreen} from "./getOpenedScreen";

export function isLazyLoading(ctx: any) {
  const openScreen = getOpenedScreen(ctx);
  return openScreen.lazyLoading && openScreen.menuItemType !== IMainMenuItemType.FormRefWithSelection;
}
