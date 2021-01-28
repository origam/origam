import {getMainMenu} from "./getMainMenu";
import {IMainMenuContent} from "../../entities/types/IMainMenu";
import { getMainMenuState } from "./getMainMenuState";
import { runInAction } from "mobx";

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

function getAllParents(node: any) {
  const parents: any[] = [];
  let cn = node;
  while (cn !== undefined) {
    parents.push(cn);
    cn = cn.parent;
  }
  parents.reverse();
  return parents.slice(2); // Strip out root and Menu node
}
