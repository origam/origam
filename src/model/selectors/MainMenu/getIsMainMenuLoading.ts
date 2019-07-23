import { getMainMenu } from "./getMainMenu";

export function getIsMainMenuLoading(ctx: any) {
  const mainMenu = getMainMenu(ctx);
  return mainMenu ? mainMenu.isLoading : false;
}