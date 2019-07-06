import { IMainMenu, ILoadingMainMenu } from "../../types/IMainMenu";
import { getWorkbench } from "../getWorkbench";

export function getMainMenu(
  ctx: any
): IMainMenu | ILoadingMainMenu | undefined {
  return getWorkbench(ctx)!.mainMenu;
}
