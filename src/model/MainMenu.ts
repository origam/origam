import { IMainMenu, IMainMenuData } from "./types/IMainMenu";

export class MainMenu implements IMainMenu {
  MainMenu: "MainMenu" = "MainMenu";
  
  constructor(data: IMainMenuData) {
    Object.assign(this, data);
  }

  menuUI: any;
  parent?: any;
}
