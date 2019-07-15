export interface IMainMenuData {
  menuUI: any;
}

export interface IMainMenu extends IMainMenuData {
  $type_IMainMenu: 1;
  isLoading: false;

  parent?: any;
}

export interface ILoadingMainMenu {
  $type_ILoadingMainMenu: 1;

  isLoading: true;
  parent?: any;
}

export const isIMainMenu = (o: any): o is IMainMenu => o.$type_IMainMenu;
export const isILoadingMainMenu = (o: any): o is IMainMenu =>
  o.$type_ILoadingMainMenu;
