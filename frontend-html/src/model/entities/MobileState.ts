import { observable } from "mobx";
import { MainPageContents } from "gui/connections/MobileComponents/MobileMain";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";

export class MobileState {
  @observable
  tabsDroppedDown: boolean = false;

  _workbench: IWorkbench | undefined;

  previousMainPageContents: undefined | MainPageContents;

  @observable
  mainPageContents = MainPageContents.Screen

  set workbench(workbench: IWorkbench){
    let workbenchLifecycle = getWorkbenchLifecycle(workbench);
    workbenchLifecycle.addMainMenuItemClickHandler(
      () => this.mainPageContents = MainPageContents.Screen
    );
    this._workbench = workbench;
  }

  async close() {
    if(this.mainPageContents === MainPageContents.Screen){
      const activeScreen = getOpenedNonDialogScreenItems(this._workbench)
        .find(screen => screen.isActive);
      if(activeScreen){
        await onScreenTabCloseClick(activeScreen)(null);
      }
    }else{
      this.mainPageContents = MainPageContents.Screen;
    }
  }
}