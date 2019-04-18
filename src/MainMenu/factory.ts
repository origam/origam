import { findStopping, parseBoolean } from "../utils/xml";
import { Command, Submenu } from "./MainMenu";
import { IAOnItemClick } from "./types";
import { ML } from "../utils/types";


export const findMenu = (node: any) =>
  findStopping(node, (n: any) => n.name === "Menu")[0];

export const findMenuItems = (node: any) =>
  findStopping(node, (n: any) => n.name === "Command" || n.name === "Submenu");

export function recursiveBuildMenuItems(node: any,aOnItemClick: ML<IAOnItemClick>) {
  function recursive(n: any): any {
    const menuItems = findMenuItems(n);
    switch (n.name) {
      case "Command":
        return new Command({
          id: n.attributes.id,
          icon: n.attributes.icon,
          label: n.attributes.label,
          showInfoPanel: n.attributes.showInfoPanel,
          commandType: n.attributes.type,
          aOnItemClick
        });
      case "Submenu":
        return new Submenu({
          id: n.attributes.id,
          icon: n.attributes.icon,
          label: n.attributes.label,
          isHidden: parseBoolean(n.attributes.isHidden),
          children: menuItems.map(item => recursive(item))
        });
      default:
        return menuItems.map(item => recursive(item));
    }
  }
  return recursive(node);
}

export function buildMainMenu(xmlObj: any, aOnItemClick: ML<IAOnItemClick>) {
  const menuRoot = findMenu(xmlObj);
  const menu = recursiveBuildMenuItems(menuRoot, aOnItemClick);
  return menu;
}

export class MainMenuFactory {
  constructor(public P:{aOnItemClick: ML<IAOnItemClick>}) {}

  create(menuObj: any) {
    const menu = buildMainMenu(menuObj, this.P.aOnItemClick);
    return menu;
  }


}