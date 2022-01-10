/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

import { ReactNode } from "react";
import { IDataView } from "model/entities/types/IDataView";
import { action, observable } from "mobx";
import { MobileState } from "model/entities/MobileState/MobileState";
import { BreadCrumbNode } from "gui/connections/MobileComponents/Navigation/BreadCrumbs";
import { IFormScreen } from "model/entities/types/IFormScreen";
import { getActiveScreen } from "model/selectors/getActiveScreen";

export interface INavigationNode {
  readonly name: string;
  readonly children: INavigationNode[];
  parent: INavigationNode | undefined;
  readonly parentChain: INavigationNode[];
  showDetailLinks(): boolean;
  readonly element: ReactNode;
  readonly id: string;
  dataView: IDataView | undefined;
  formScreen: IFormScreen | undefined;

  equals(other: INavigationNode): boolean;
  addChild(node: INavigationNode): void;
  removeChild(node: INavigationNode): void;
  merge(other: INavigationNode): void;
}

export class NavigationNode implements INavigationNode {
  private _children: NavigationNode[] = [];
  private _name: string = "";
  private _dataView: IDataView | undefined;
  formScreen: IFormScreen | undefined;
  parent: NavigationNode | undefined;

  get dataView(): IDataView | undefined {
    if(this._dataView){
      return this._dataView;
    }
    if(!this.parent && (this.formScreen?.rootDataViews?.length ?? 0) > 0){
      return this.formScreen!.rootDataViews[0]!
    }
      return undefined;
  }

  set dataView(value: IDataView | undefined) {
    this._dataView = value;
  }
  showDetailLinks(){
    if(!this._dataView){
      return true;
    }
    return this._dataView.isFormViewActive();
  }

  public id: string = "";
  public element: ReactNode = null as any;

  get name(): string {
    let finalName = !this._name ? this.id : this._name;
    if(!this.parent && this.formScreen){
      finalName = getActiveScreen(this.formScreen)
        ?.tabTitle
        ?? finalName;
    }
    return finalName
  }

  set name(value: string) {
    this._name = value;
  }

  get children() {
    return this._children;
  }

  get parentChain() {
    let parent = this.parent;
    const chain: INavigationNode[] = [this];
    while (parent) {
      chain.push(parent);
      parent = parent.parent;
    }
    return chain.reverse();
  }

  addChild(node: NavigationNode) {
    this._children.push(node);
    node.parent = this;
  }

  removeChild(node: NavigationNode) {
    this._children.remove(node);
    node.parent = undefined;
  }

  merge(other: NavigationNode) {
    this.element = other.element;
    for (const child of [...other.children]) {
      other.removeChild(child);
      this.addChild(child);
    }
    this.id = other.id;
    this._name = other._name;
    this.showDetailLinks = other.showDetailLinks;
    this._dataView = other._dataView;
  }

  equals(other: INavigationNode): boolean {
    return this.id === other.id;
  }
}

export class NavigatorState{

  @observable
  currentNode: INavigationNode;

  constructor(private mobileState: MobileState, node: INavigationNode) {
    this.currentNode = node;
  }

  @action
  onNodeClick(node: INavigationNode){
    if(this.currentNode === node){
      if(this.currentNode.dataView?.isFormViewActive()){
        this.currentNode.dataView?.activateTableView?.();
      }
      return;
    }
    this.currentNode = node;
    this.mobileState.activeDataViewId = node.dataView?.id;
    const nodes = this.currentNode.parentChain
      .map(navNode => new BreadCrumbNode(navNode.name, () => this.onNodeClick(navNode)));
    this.mobileState.breadCrumbsState.setActiveBreadCrumbList(nodes);
    this.mobileState.breadCrumbsState.addDetailBreadCrumbNode(this.currentNode.dataView!);
  }
}