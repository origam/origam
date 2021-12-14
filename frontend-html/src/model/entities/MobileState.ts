import { observable } from "mobx";
import { MainPageContents } from "gui/connections/MobileComponents/MobileMain";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";

export class MobileState {
  @observable
  tabsDroppedDown: boolean = false;

  previousMainPageContents: undefined | MainPageContents;

  @observable
  mainPageContents = MainPageContents.Screen

  set workbench(workbench: IWorkbench){
    let workbenchLifecycle = getWorkbenchLifecycle(workbench);
    workbenchLifecycle.addMainMenuItemClickHandler(
      () => this.mainPageContents = MainPageContents.Screen
    );
  }

  close() {
    this.mainPageContents = MainPageContents.Screen;
  }
}