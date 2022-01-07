import { action, computed, observable, reaction } from "mobx";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { IBreadCrumbNode, RootBreadCrumbNode } from "gui/connections/MobileComponents/Navigation/BreadCrumbs";
import { IDataView } from "model/entities/types/IDataView";
import { T } from "utils/translation";
import { getDialogStack } from "model/selectors/getDialogStack";
import { IFormScreen } from "model/entities/types/IFormScreen";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";

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
    this.start();
  }

  // It is ok for these reactions to run indefinitely because the MobileState is never disposed. Hence, no disposers here.
  start() {
    reaction(() => {
        const openedScreenItems = getOpenedNonDialogScreenItems(this._workbench);
        return openedScreenItems.find(item => item.isActive)?.content?.formScreen;
      },
      (activeFormScreen) => this.breadCrumbsState.updateBreadCrumbs(activeFormScreen),
      {fireImmediately: true}
    );

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
}


export class BreadCrumbsState {

  workbench: IWorkbench | undefined;

  @observable
  activeFormScreen:  IFormScreen | undefined;

  @observable
  openScreenBreadCrumbs = new Map<IFormScreen, IBreadCrumbNode[]>();

  setActiveBreadCrumbList(nodes: IBreadCrumbNode[], ctx: any){
    this.activeFormScreen = getFormScreen(ctx);
    this.openScreenBreadCrumbs.set(this.activeFormScreen, nodes);
  }

  @computed
  get activeBreadCrumbList(){
    return this.activeFormScreen ? this.openScreenBreadCrumbs.get(this.activeFormScreen) ?? [] : [];
  }

  @action
  updateBreadCrumbs(activeFormScreen: IFormScreen | undefined) {
    this.activeFormScreen = activeFormScreen;
    if(!activeFormScreen || this.openScreenBreadCrumbs.has(activeFormScreen)){
      return;
    }
    this.resetBreadCrumbs(activeFormScreen);
  }

  private resetBreadCrumbs(activeFormScreen: IFormScreen){
    const breadCrumbCaption = () => this.workbench
      ? getOpenedNonDialogScreenItems(this.workbench).find(item => item.isActive)?.tabTitle ?? ""
      : "";
    this.openScreenBreadCrumbs.set(activeFormScreen, []);
    const activeScreen = getOpenedNonDialogScreenItems(this.workbench).find(item => item.isActive);
    if (!activeScreen) {
      return;
    }
    const rootBreadCrumbNode = new RootBreadCrumbNode(breadCrumbCaption);
    this.openScreenBreadCrumbs
      .get(activeFormScreen)!
      .push(rootBreadCrumbNode);

    if ((activeFormScreen?.rootDataViews?.length ?? 0) > 0 && activeFormScreen?.uiRootType !== "Tab") {
      const dataView = activeFormScreen?.rootDataViews[0]!;
      this.addDetailBreadCrumbNodeToRoot(dataView);
    }
  }

  @action
  addDetailBreadCrumbNodeToRoot(dataView: IDataView) {
    if (this.activeBreadCrumbList?.length === 1) {
      this.activeBreadCrumbList[0].onClick = () => dataView.activateTableView?.();
      this.addDetailBreadCrumbNode(dataView);
    }
  }

  @action
  addDetailBreadCrumbNode(dataView: IDataView) {
    const node = {
      caption: T("Detail", "mobile_detail_navigation"),
      isVisible: () => dataView?.isFormViewActive()!,
      onClick: () => {
      }
    };
    // this.activeBreadCrumbList?.push(node);
    this.openScreenBreadCrumbs
      .get(this.activeFormScreen!)!
      .push(node);
  }
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