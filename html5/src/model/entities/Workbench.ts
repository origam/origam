import { IWorkbench, IWorkbenchData } from "./types/IWorkbench";
import { IMainMenuEnvelope } from "./types/IMainMenu";
import { IWorkbenchLifecycle } from "./types/IWorkbenchLifecycle";
import { action } from "mobx";
import { IClientFulltextSearch } from "./types/IClientFulltextSearch";

export class Workbench implements IWorkbench {
  $type_IWorkbench: 1 = 1;

  constructor(data: IWorkbenchData) {
    Object.assign(this, data);
    this.workbenchLifecycle.parent = this;
    this.clientFulltextSearch.parent = this;
  }

  workbenchLifecycle: IWorkbenchLifecycle = null as any;
  clientFulltextSearch: IClientFulltextSearch = null as any;
  mainMenuEnvelope: IMainMenuEnvelope = null as any;

  @action.bound
  run(): void {
    this.workbenchLifecycle.run();
  }

  parent?: any;

  /*@action.bound setMainMenu(mainMenu: ILoadingMainMenu | IMainMenu) {
    this.mainMenu = mainMenu;
    if (!mainMenu.isLoading) {
      this.clientFulltextSearch.indexMainMenu((mainMenu as IMainMenu).menuUI);
    }

    mainMenu.parent = this;
  }*/
}
