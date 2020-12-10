import { IMainMenuItemType } from "model/entities/types/IMainMenu";
import {getOpenedScreen} from "./getOpenedScreen";

export function getDontRequestData(ctx: any) {
  const openScreen = getOpenedScreen(ctx);
  return openScreen.dontRequestData && openScreen.menuItemType !== IMainMenuItemType.FormRefWithSelection;
}
