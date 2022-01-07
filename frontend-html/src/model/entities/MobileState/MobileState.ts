import { observable, reaction } from "mobx";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { getDialogStack } from "model/selectors/getDialogStack";
import { IFormScreen } from "model/entities/types/IFormScreen";
import { BreadCrumbsState } from "model/entities/MobileState/BreadCrumbsState";
import { IMobileLayoutState, MenuLayoutState, ScreenLayoutState } from "model/entities/MobileState/MobileLayoutState";

export class MobileState {
  _workbench: IWorkbench | undefined;

  @observable
  layoutState: IMobileLayoutState = new ScreenLayoutState()

  @observable
  activeDataViewId: string | undefined;

  breadCrumbsState = new BreadCrumbsState()

  initialize(workbench: IWorkbench) {
    let workbenchLifecycle = getWorkbenchLifecycle(workbench);
    workbenchLifecycle.mainMenuItemClickHandler.add(
      () => this.layoutState = new ScreenLayoutState()
    );
    this._workbench = workbench;
    this.breadCrumbsState.workbench = workbench;
    this.breadCrumbsState.updateBreadCrumbs();
    this.start();
  }

  // It is ok for these reactions to run indefinitely because the MobileState is never disposed. Hence, no disposers here.
  start() {
    reaction(
      () => {
        const openedScreenItems = getOpenedNonDialogScreenItems(this._workbench);
        return {
          activeScreen: !!openedScreenItems.find(item => item.isActive),
          dialogOpen: getDialogStack(this._workbench).isAnyDialogShown
        };
      },
      (args) => {
        if (!args.activeScreen && this.layoutState instanceof ScreenLayoutState) {
          this.layoutState = new MenuLayoutState();
          return;
        }
        if (args.activeScreen && !args.dialogOpen && this.layoutState instanceof MenuLayoutState) {
          this.layoutState = new ScreenLayoutState();
        }
      },
      {fireImmediately: true}
    );
  }

  async close() {
    this.layoutState = await this.layoutState.close(this._workbench);
  }

  hamburgerClick() {
    this.layoutState = this.layoutState.hamburgerClick();
  }

  onFormClose(formScreen: IFormScreen | undefined) {
    if(!formScreen){
      return;
    }
    this.breadCrumbsState.onFormClose(formScreen);
  }
}

