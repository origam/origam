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
import { action, computed, observable } from "mobx";
import { MobileState } from "model/entities/MobileState/MobileState";
import { BreadCrumbNode } from "gui/connections/MobileComponents/Navigation/BreadCrumbs";
import { IFormScreen } from "model/entities/types/IFormScreen";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import { getFieldErrorMessage } from "model/selectors/DataView/getFieldErrorMessage";

export interface INavigationNode {
  readonly name: string;
  readonly children: INavigationNode[];
  readonly allChildren: INavigationNode[];
  parent: INavigationNode | undefined;
  readonly parentChain: INavigationNode[];
  readonly element: ReactNode;
  readonly id: string;
  dataView: IDataView | undefined;
  formScreen: IFormScreen | undefined;

  equals(other: INavigationNode): boolean;
  addChild(node: INavigationNode): void;
  addChildren(nodes: (INavigationNode | undefined)[]): void;
  removeChild(node: INavigationNode): void;
  merge(other: INavigationNode): void;
  readonly navigationDisabled: boolean;
}

export class NavigationNode implements INavigationNode {
  private _children: INavigationNode[] = [];
  private _name: string = "";
  private _dataView: IDataView | undefined;
  formScreen: IFormScreen | undefined;
  parent: INavigationNode | undefined;

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

  get allChildren() {
    return this._children.flatMap(child => child.children);
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

  @computed
  get navigationDisabled(){
    if(!this.dataView?.selectedRow){
      return false;
    }
    return this.dataView.properties
      .some(prop => getFieldErrorMessage(prop!)(this.dataView!.selectedRow!, prop!))
  }

  addChild(node: INavigationNode) {
    this._children.push(node);
    node.parent = this;
  }

  addChildren(nodes: (INavigationNode | undefined)[]) {
    for (const node of nodes) {
      if(node){
        this.addChild(node);
      }
    }
  }

  removeChild(node: INavigationNode) {
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

  @computed
  get navigationDisabled(){
    return this.currentNode.parentChain
      .concat(this.currentNode.allChildren)
      .some(node => node.navigationDisabled);
  }

  @action
  onLinkClick(node: INavigationNode) {
    if(this.navigationDisabled){
      return;
    }
    this.onNodeClick(node);
    if(!node.dataView?.isHeadless){
      this.currentNode.dataView?.activateTableView?.();
    }
  }

  @action
  private onNodeClick(node: INavigationNode){
    this.currentNode = node;
    this.mobileState.activeDataViewId = node.dataView?.id;
    const nodes = this.currentNode.parentChain
      .map(navNode =>
        new BreadCrumbNode(
          navNode.name,
          navNode.id,
          () => this.onBreadCrumbClick(navNode),
          false));
    if(node.dataView?.isHeadless && nodes.length > 1){
      nodes[nodes.length - 1 ].disabled = true;
    }
    this.mobileState.breadCrumbsState.setActiveBreadCrumbList(nodes);
    if(this.currentNode.element){
      this.mobileState.breadCrumbsState.addDetailBreadCrumbNode(this.currentNode.dataView!);
    }
  }

  @action
  onBreadCrumbClick(node: INavigationNode){
    if(this.navigationDisabled){
      return;
    }
    if(this.currentNode === node){
      if(this.currentNode.dataView?.isFormViewActive()){
        this.currentNode.dataView?.activateTableView?.();
      }
      return;
    }
    this.onNodeClick(node);
    this.currentNode.dataView?.activateFormView?.();
  }
}