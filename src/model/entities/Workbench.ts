import {IWorkbench, IWorkbenchData} from "./types/IWorkbench";
import {IMainMenuEnvelope} from "./types/IMainMenu";
import {IWorkbenchLifecycle} from "./types/IWorkbenchLifecycle";
import {action, computed, observable} from "mobx";
import {IClientFulltextSearch} from "./types/IClientFulltextSearch";
import {IOpenedScreens} from "./types/IOpenedScreens";
import {IWorkQueues} from "./types/IWorkQueues";
import {IRecordInfo} from "./types/IRecordInfo";

export class Workbench implements IWorkbench {
  $type_IWorkbench: 1 = 1;

  constructor(data: IWorkbenchData) {
    Object.assign(this, data);
    this.workbenchLifecycle.parent = this;
    this.clientFulltextSearch.parent = this;
    this.openedScreens.parent = this;
    this.openedDialogScreens.parent = this;
    this.workQueues.parent = this;
    this.recordInfo.parent = this;
  }

  workbenchLifecycle: IWorkbenchLifecycle = null as any;
  clientFulltextSearch: IClientFulltextSearch = null as any;
  mainMenuEnvelope: IMainMenuEnvelope = null as any;
  openedScreens: IOpenedScreens = null as any;
  openedDialogScreens: IOpenedScreens = null as any;
  workQueues: IWorkQueues = null as any;
  recordInfo: IRecordInfo = null as any;

  @observable isFullScreen: boolean = false;

  @action.bound setFullscreen(state: boolean) {
    this.isFullScreen = state;
  }

  @computed get openedScreenIdSet() {
    return new Set(this.openedScreens.items.map(item => item.menuItemId));
  }

  *run(): Generator {
    yield* this.workbenchLifecycle.run();
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
