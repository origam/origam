import { IMainMenu, IMainMenuData, ILoadingMainMenu } from "./types/IMainMenu";

export class MainMenu implements IMainMenu {
  $type: "CMainMenu" = "CMainMenu";
  isLoading: false = false;
  
  constructor(data: IMainMenuData) {
    Object.assign(this, data);
  }

  menuUI: any;
  parent?: any;
}

export class LoadingMainMenu implements ILoadingMainMenu {
  $type: "CLoadingMainMenu" = "CLoadingMainMenu";
  isLoading: true = true;
}
