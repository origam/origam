import { getMainMenu } from "./getMainMenu";
import { IMainMenu } from "../../entities/types/IMainMenu";

export function getMainMenuUI(ctx: any) {
  return (getMainMenu(ctx) as IMainMenu)!.menuUI;
}