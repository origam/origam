/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
import { IWorkbench } from "model/entities/types/IWorkbench";
import { action, computed, observable } from "mobx";
import { IFormScreen } from "model/entities/types/IFormScreen";
import { IBreadCrumbNode, RootBreadCrumbNode } from "gui/connections/MobileComponents/Navigation/BreadCrumbs";
import { IDataView } from "model/entities/types/IDataView";
import { T } from "utils/translation";
import { getActiveScreen } from "model/selectors/getActiveScreen";

const detailId = "Detail";

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
      if(dataView.isHeadless && this.activeBreadCrumbList?.length === 2){
        this.activeBreadCrumbList[0].disabled = true;
      }
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
  removeDetailNode(){
    if (this.activeBreadCrumbList?.length === 2 && this.activeBreadCrumbList[1].id === detailId) {
      this.activeBreadCrumbList.pop();
      this.activeBreadCrumbList[0].disabled = false;
    }
  }

  @action
  addDetailBreadCrumbNode(dataView: IDataView) {
    this.activeBreadCrumbList?.push({
      caption: T("Detail", "mobile_detail_navigation"),
      id: detailId,
      isVisible: () => dataView?.isFormViewActive()!,
      onClick: () => {},
      disabled: false
    });
  }

  @computed
  get visibleNodes(){
    return (this.activeBreadCrumbList ?? [])
      .filter(x => x.isVisible());
  }

  get canGoBack() {
    return this.visibleNodes.length >= 2;
  }

  @action
  goBack(){
    if(!this.canGoBack){
      return;
    }
    const previousElement = this.visibleNodes[this.visibleNodes!.length - 2];
    previousElement.onClick();
  }

  onFormClose(formScreen: IFormScreen) {
    this.openScreenBreadCrumbs.delete(formScreen);
  }
}