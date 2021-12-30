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

export interface INavigationNode {
  readonly name: string;
  readonly children: INavigationNode[];
  parent: INavigationNode | undefined;
  readonly parentChain: INavigationNode[];
  readonly showDetailLinks: boolean;
  readonly element: ReactNode;
  readonly id: string;

  equals(other: INavigationNode): boolean;
  addChild(node: INavigationNode): void;
  removeChild(node: INavigationNode): void;
  merge(other: INavigationNode): void;
}

export class NavigationNode2 implements INavigationNode {
  _children: NavigationNode2[] = [];
  private _name: string = "";

  parent: NavigationNode2 | undefined;
  readonly showDetailLinks = true;

  public id: string = "";
  public element: ReactNode = null as any;

  get name(): string {
    return !this._name ? this.id : this._name;
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

  addChild(node: NavigationNode2) {
    this._children.push(node);
    node.parent = this;
  }

  removeChild(node: NavigationNode2) {
    this._children.remove(node);
    node.parent = undefined;
  }

  merge(other: NavigationNode2) {
    this.element = other.element;
    for (const child of [...other.children]) {
      other.removeChild(child);
      this.addChild(child);
    }
    this.id = other.id;
    this._name = other._name;
  }

  equals(other: INavigationNode): boolean {
    return this.id === other.id;
  }
}