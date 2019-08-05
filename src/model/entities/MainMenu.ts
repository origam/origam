import { IMainMenuContent, IMainMenuData, IMainMenuEnvelope, IMainMenu } from "./types/IMainMenu";
import { action, observable } from "mobx";
import { proxyEnrich } from "utils/esproxy";

export class MainMenuContent implements IMainMenuContent {
  $type_IMainMenuContent: 1 = 1;

  constructor(data: IMainMenuData) {
    Object.assign(this, data);
  }

  menuUI: any;
  parent?: any;
}

export class MainMenuEnvelope implements IMainMenuEnvelope {
  $type_IMainMenuEnvelope: 1 = 1;

  @observable mainMenu?: IMainMenu | undefined;
  @observable isLoading: boolean = false;
  
  @action.bound
  setMainMenu(mainMenu: IMainMenuContent | undefined): void {
    if (mainMenu) {
      mainMenu.parent = this;
      this.mainMenu = proxyEnrich<IMainMenuEnvelope, IMainMenuContent>(mainMenu);
    } else {
      this.mainMenu = undefined;
    }
  }

  @action.bound setLoading(state: boolean) {
    this.isLoading = state;
  }

  parent?: any;
}
