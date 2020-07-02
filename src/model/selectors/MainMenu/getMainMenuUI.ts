import {getMainMenu} from "./getMainMenu";
import {IMainMenuContent} from "../../entities/types/IMainMenu";

export function getMainMenuUI(ctx: any) {
  return (getMainMenu(ctx) as IMainMenuContent)!.menuUI;
}