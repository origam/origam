import {getMainMenu} from "./getMainMenu";
import {IMainMenuContent} from "../../entities/types/IMainMenu";
import { getMainMenuState } from "./getMainMenuState";
import { runInAction } from "mobx";
import { getAllParents } from "model/selectors/MainMenu/menuNode";

export function getMainMenuUI(ctx: any) {
  return (getMainMenu(ctx) as IMainMenuContent)!.menuUI;
}

export function openSingleMenuFolder(folderNode: any, ctx: any){
  if(folderNode.name !== "Submenu"){
    return;
  }
  const mainMenuState = getMainMenuState(ctx);

  runInAction(() => {
    mainMenuState.closeAll();
    const nodesToOpen = getAllParents(folderNode);
    for (const node of nodesToOpen) {
      mainMenuState.setIsOpen(node.attributes.id, true);
    }
  });
}
