import { action, observable } from "mobx";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import {
  IBreadCrumbNode,
  RootBreadCrumbNode
} from "gui/connections/MobileComponents/Navigation/BreadCrumbs";
import { IDataView } from "model/entities/types/IDataView";

export class MobileState {
  _workbench: IWorkbench | undefined;

  @observable
  layoutState: IMobileLayoutState = new ScreenLayoutState()

  @observable
  activeDataViewId: string | undefined;

  breadCrumbsState = new BreadCrumbsState()

  set workbench(workbench: IWorkbench){
    let workbenchLifecycle = getWorkbenchLifecycle(workbench);
    workbenchLifecycle.mainMenuItemClickHandler.add(
      () => this.layoutState = new ScreenLayoutState()
    );
    this._workbench = workbench;
    this.breadCrumbsState.workbench = workbench;
  }

  async close() {
    this.layoutState = await this.layoutState.close(this._workbench);
  }

  hamburgerClick() {
    this.layoutState = this.layoutState.hamburgerClick();
  }
}


export class BreadCrumbsState{

  workbench: IWorkbench |undefined;

  @action
  resetBreadCrumbs(){
    const breadCrumbCaption = () => this.workbench
      ? getOpenedNonDialogScreenItems(this.workbench).find(item => item.isActive)?.tabTitle ?? ""
      : "";
    this.breadCrumbList.length = 0;
    const activeScreen = getOpenedNonDialogScreenItems(this.workbench).find(item => item.isActive);

    this.breadCrumbList.push(new RootBreadCrumbNode(breadCrumbCaption));
    if((activeScreen?.content?.formScreen?.rootDataViews?.length ?? 0) > 0){
      const dataView = activeScreen?.content?.formScreen?.rootDataViews[0]!;
      this.addDetailBreadCrumbNodeToRoot(dataView);
    }
  }

  @action
  addDetailBreadCrumbNodeToRoot(dataView: IDataView){
    if(this.breadCrumbList.length === 1){
      this.breadCrumbList[0].onClick = ()=> dataView.activateTableView?.();
      this.addDetailBreadCrumbNode(dataView);
    }
  }

  @action
  addDetailBreadCrumbNode(dataView: IDataView){
    this.breadCrumbList.push({
      caption: "Detail",
      isVisible: () => dataView?.isFormViewActive()!,
      onClick: () => {}
    });
  }

  @observable
  breadCrumbList: IBreadCrumbNode[] = [];
}

interface IMobileLayoutState{
  actionDropUpHidden: boolean;
  refreshButtonHidden: boolean;
  saveButtonHidden: boolean;
  showOpeTabCombo: boolean;
  showSearchButton: boolean;
  hamburgerClick(): IMobileLayoutState;
  close(ctx: any): Promise<IMobileLayoutState>;
}

export class MenuLayoutState implements IMobileLayoutState{
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpeTabCombo = false;
  showSearchButton = true;

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new ScreenLayoutState();
  }
}

export class AboutLayoutState implements IMobileLayoutState{
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpeTabCombo = false;
  showSearchButton = true;

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }
}

export class SearchLayoutState implements IMobileLayoutState{
  actionDropUpHidden = true;
  refreshButtonHidden = true;
  saveButtonHidden = true;
  showOpeTabCombo = false;
  showSearchButton = false;

  async close(ctx: any): Promise<IMobileLayoutState> {
    return new ScreenLayoutState();
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }
}

export class ScreenLayoutState implements IMobileLayoutState{
  actionDropUpHidden = false;
  refreshButtonHidden = false;
  saveButtonHidden = false;
  showOpeTabCombo = true;
  showSearchButton = true;

  async close(ctx: any): Promise<IMobileLayoutState> {
    const activeScreen = getOpenedNonDialogScreenItems(ctx)
      .find(screen => screen.isActive);
    if(activeScreen){
      await onScreenTabCloseClick(activeScreen)(null);
    }
    return this;
  }

  hamburgerClick(): IMobileLayoutState {
    return new MenuLayoutState();
  }
}