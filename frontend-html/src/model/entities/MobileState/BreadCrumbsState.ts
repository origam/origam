import { IWorkbench } from "model/entities/types/IWorkbench";
import { action, computed, observable } from "mobx";
import { IFormScreen } from "model/entities/types/IFormScreen";
import { IBreadCrumbNode, RootBreadCrumbNode } from "gui/connections/MobileComponents/Navigation/BreadCrumbs";
import { IDataView } from "model/entities/types/IDataView";
import { T } from "utils/translation";
import { getActiveScreen } from "model/selectors/getActiveScreen";

export class BreadCrumbsState {

  workbench: IWorkbench | undefined;

  @computed
  get activeFormScreen() {
    return getActiveScreen(this.workbench)?.content?.formScreen;
  }

  @observable
  openScreenBreadCrumbs = new Map<IFormScreen, IBreadCrumbNode[]>();

  @action
  setActiveBreadCrumbList(nodes: IBreadCrumbNode[]) {
    if (this.openScreenBreadCrumbs.has(this.activeFormScreen!)){
      const openNodes = this.openScreenBreadCrumbs.get(this.activeFormScreen!)!;
      openNodes.length = 0;
      nodes.forEach(node => openNodes.push(node));
    } else {
      this.openScreenBreadCrumbs.set(this.activeFormScreen!, nodes);
    }
  }

  get activeBreadCrumbList() {
    if (!this.activeFormScreen) {
      return undefined;
    }
    if (!this.openScreenBreadCrumbs.has(this.activeFormScreen)) {
      this.updateBreadCrumbs();
    }
    return this.openScreenBreadCrumbs.get(this.activeFormScreen)
  }

  @action
  updateBreadCrumbs() {
    if (!this.activeFormScreen || this.openScreenBreadCrumbs.has(this.activeFormScreen)) {
      return;
    }
    this.resetBreadCrumbs(this.activeFormScreen);
  }

  private resetBreadCrumbs(activeFormScreen: IFormScreen) {
    const breadCrumbCaption = () => this.workbench
      ? getActiveScreen(this.workbench)?.tabTitle ?? ""
      : "";
    this.openScreenBreadCrumbs.set(activeFormScreen, [new RootBreadCrumbNode(breadCrumbCaption)]);

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
    this.activeBreadCrumbList?.push({
      caption: T("Detail", "mobile_detail_navigation"),
      id: "Detail",
      isVisible: () => dataView?.isFormViewActive()!,
      onClick: () => {
      }
    });
  }

  onFormClose(formScreen: IFormScreen) {
    this.openScreenBreadCrumbs.delete(formScreen);
  }
}