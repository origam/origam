export const CMainMenu = "CMainMenu";
export const CLoadingMainMenu = "CLoadingMainMenu";

export interface IMainMenuData {
  menuUI: any;
}

export interface IMainMenu extends IMainMenuData {
  $type: typeof CMainMenu;
  isLoading: false;

  parent?: any
}

export interface ILoadingMainMenu {
  $type: typeof CLoadingMainMenu;
  isLoading: true;
  parent?: any;
}