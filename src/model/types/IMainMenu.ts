export const MainMenu = "MainMenu";
export const LoadingMainMenu = "LoadingMainMenu";

export interface IMainMenuData {
  menuUI: any;
}

export interface IMainMenu extends IMainMenuData {
  MainMenu: typeof MainMenu;
  parent?: any
}

export interface ILoadingMainMenu {
  LoadingMainMenu: typeof LoadingMainMenu;
  parent?: any;
}