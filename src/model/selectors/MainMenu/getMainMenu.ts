import {IMainMenu} from "../../entities/types/IMainMenu";
import {getMainMenuEnvelope} from "./getMainMenuEnvelope";

export function getMainMenu(ctx: any): IMainMenu | undefined {
  return getMainMenuEnvelope(ctx).mainMenu;
}
