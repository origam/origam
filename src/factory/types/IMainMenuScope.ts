import { IMainMenu, IAOnItemClick, IMainMenuFactory } from "../../MainMenu/types";

export interface IMainMenuScope {
  mainMenu: IMainMenu;
  aOnItemClick: IAOnItemClick;
  mainMenuFactory: IMainMenuFactory;
}