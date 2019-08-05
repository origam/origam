import { IMainMenuContent, IMainMenu } from "../../entities/types/IMainMenu";
import { getWorkbench } from "../getWorkbench";
import { getMainMenuEnvelope } from "./getMainMenuEnvelope";

export function getMainMenu(ctx: any): IMainMenu | undefined {
  return getMainMenuEnvelope(ctx).mainMenu;
}
