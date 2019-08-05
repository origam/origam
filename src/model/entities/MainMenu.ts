import { IMainMenu, IMainMenuData, ILoadingMainMenu } from "./types/IMainMenu";

export class MainMenu implements IMainMenu {
  $type_IMainMenu: 1 = 1;

  constructor(data: IMainMenuData) {
    Object.assign(this, data);
  }

  isLoading: false = false;
  menuUI: any;
  parent?: any;

  onItemClick(args: { event: any; item: any }): void {
    console.log("MainMenu item clicked:", args.item);
  }
}

export class LoadingMainMenu implements ILoadingMainMenu {
  $type_ILoadingMainMenu: 1 = 1;

  isLoading: true = true;
  parent?: any;
}
