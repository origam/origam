import { IWorkbench, IWorkbenchData, CWorkbench } from "./types/IWorkbench";
import { ILoadingMainMenu, IMainMenu } from "./types/IMainMenu";
import { IWorkbenchLifecycle } from "./types/IWorkbenchLifecycle";
import { action, observable } from "mobx";

export class Workbench implements IWorkbench {
  $type: "CWorkbench" = "CWorkbench";

  constructor(data: IWorkbenchData) {
    Object.assign(this, data);
    this.workbenchLifecycle.parent = this;
  }

  workbenchLifecycle: IWorkbenchLifecycle = null as any;
  @observable mainMenu?: ILoadingMainMenu | IMainMenu | undefined;

  @action.bound
  run(): void {
    this.workbenchLifecycle.run();
  }

  parent?: any;

  @action.bound setMainMenu(mainMenu: ILoadingMainMenu | IMainMenu) {
    this.mainMenu = mainMenu;
    mainMenu.parent = this;
  }
}
