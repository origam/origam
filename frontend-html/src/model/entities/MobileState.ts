import { action, observable, reaction } from "mobx";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { IBreadCrumbNode, RootBreadCrumbNode } from "gui/connections/MobileComponents/Navigation/BreadCrumbs";
import { IDataView } from "model/entities/types/IDataView";
import { T } from "utils/translation";
import { getDialogStack } from "model/selectors/getDialogStack";

export class MobileState {
  _workbench: IWorkbench | undefined;

  @observable
  layoutState: IMobileLayoutState = new ScreenLayoutState()

  @observable
  activeDataViewId: string | undefined;

  breadCrumbsState = new BreadCrumbsState()

  set workbench(workbench: IWorkbench) {
    let workbenchLifecycle = getWorkbenchLifecycle(workbench);
    workbenchLifecycle.mainMenuItemClickHandler.add(
      () => this.layoutState = new ScreenLayoutState()
    );
    this._workbench = workbench;
    this.breadCrumbsState.workbench = workbench;

    const openedScreenItems = getOpenedNonDialogScreenItems(this._workbench);
    if (openedScreenItems.length > 0) {
      this.breadCrumbsState.resetBreadCrumbs();
    }

    reaction(
      () => {
        const openedScreenItems = getOpenedNonDialogScreenItems(this._workbench);
        return {activeScreen: !!openedScreenItems.find(item => item.isActive),
                dialogOpen: getDialogStack(this._workbench).isAnyDialogShown};
      },
      (args) => {
        if (!args.activeScreen && this.layoutState instanceof ScreenLayoutState) {
          this.layoutState = new MenuLayoutState();
          return;
        }
        if(args.activeScreen && !args.dialogOpen && this.layoutState instanceof MenuLayoutState){
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
}


export class BreadCrumbsState {

  workbench: IWorkbench | undefined;

  @action
  resetBreadCrumbs() {
    const breadCrumbCaption = () => this.workbench
      ? getOpenedNonDialogScreenItems(this.workbench).find(item => item.isActive)?.tabTitle ?? ""
      : "";
    this.breadCrumbList.length = 0;
    const activeScreen = getOpenedNonDialogScreenItems(this.workbench).find(item => item.isActive);
    if (!activeScreen) {
      return;
    }
    this.breadCrumbList.push(new RootBreadCrumbNode(breadCrumbCaption));

    const reactionDisposer = reaction(
      () => activeScreen.content.formScreen,
      (formScreen) => {
        if ((formScreen?.rootDataViews?.length ?? 0) > 0) {
          const dataView = activeScreen?.content?.formScreen?.rootDataViews[0]!;
          this.addDetailBreadCrumbNodeToRoot(dataView);
          reactionDisposer();
        }
      },
      {fireImmediately: true}
    )
  }

  @action
  addDetailBreadCrumbNodeToRoot(dataView: IDataView) {
    if (this.breadCrumbList.length === 1) {
      this.breadCrumbList[0].onClick = () => dataView.activateTableView?.();
      this.addDetailBreadCrumbNode(dataView);
    }
  }

  @action
  addDetailBreadCrumbNode(dataView: IDataView) {
    this.breadCrumbList.push({
      caption: T("Detail", "mobile_detail_navigation"),
      isVisible: () => dataView?.isFormViewActive()!,
      onClick: () => {
      }
    });
  }

  @observable
  breadCrumbList: IBreadCrumbNode[] = [];
}

interface IMobileLayoutState {
  actionDropUpHidden: boolean;
  refreshButtonHidden: boolean;
  saveButtonHidden: boolean;
  showOpenTabCombo: boolean;
  showSearchButton: boolean;
  showHamburgerMenuButton: boolean;
  heading: string;

  showCloseButton(someScreensAreOpen: boolean): boolean;

  hamburgerClick(): IMobileLayoutState;

  close(ctx: any): Promise<IMobileLayoutState>;
}

export class MenuLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpenTabCombo = false;
  showSearchButton = true;
  showHamburgerMenuButton = false;
  heading = T("Menu", "menu");

  showCloseButton(someScreensAreOpen: boolean) {
    return someScreensAreOpen;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new ScreenLayoutState();
  }
}

export class AboutLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpenTabCombo = false;
  showSearchButton = true;
  showHamburgerMenuButton = true;
  heading = T("About", "about_application");

  showCloseButton(someScreensAreOpen: boolean) {
    return true;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }
}

export class SearchLayoutState implements IMobileLayoutState {
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpenTabCombo = false;
  showSearchButton = false;
  showHamburgerMenuButton = true;
  heading = T("Search", "mobile_search_title");

  showCloseButton(someScreensAreOpen: boolean) {
    return true;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }
}

export class ScreenLayoutState implements IMobileLayoutState {
  actionDropUpHidden = false;
  refreshButtonHidden = false;
  saveButtonHidden = false;
  showOpenTabCombo = true;
  showSearchButton = true;
  showHamburgerMenuButton = true;
  heading = "";

  showCloseButton(someScreensAreOpen: boolean) {
    return true;
  }

  async close(ctx: any): Promise<IMobileLayoutState> {
    const activeScreen = getOpenedNonDialogScreenItems(ctx)
      .find(screen => screen.isActive);
    if (activeScreen) {
      await onScreenTabCloseClick(activeScreen)(null);
      const stillOpenScreens = getOpenedNonDialogScreenItems(ctx);
      if (stillOpenScreens.length === 0) {
        return new MenuLayoutState();
      }
    }
    return this;
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }
}