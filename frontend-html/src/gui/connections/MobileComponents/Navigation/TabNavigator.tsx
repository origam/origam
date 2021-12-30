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

import React from "react";
import SN from "gui/connections/MobileComponents/Navigation/NavigationButton.module.scss";
import { INavigationNode, TabNavigationNode } from "gui/connections/MobileComponents/Navigation/NavigationNode";
import { NavigationButton } from "gui/connections/MobileComponents/Navigation/NavigationButton";
import { DetailNavigator } from "gui/connections/MobileComponents/Navigation/DetailNavigator";
import { observer } from "mobx-react";
import { observable } from "mobx";

@observer
export class TabNavigator extends React.Component<{
  // name: string;
  // nodes: TabNavigationNode[];
  rootNode: INavigationNode;
}> {

  @observable
  currentNode: INavigationNode = this.props.rootNode;// new RootNavigationNode({name: this.props.name, children: this.props.nodes});

  render() {
    if(!this.currentNode){
      return null;
    }
    if (isRootNavigationNode(this.currentNode)) {
      return (
        <div className={SN.navigationButtonContainer}>
          {this.currentNode.children.map(node =>
            <NavigationButton
              key={node.id}
              label={node.name}
              onClick={() => {
                this.currentNode = node
              }
              }
            />)
          }
        </div>
      );
    } else {
      return <DetailNavigator node={this.currentNode} onNodeClick={node => this.currentNode = node}/>
    }
  }
}

export class RootNavigationNode implements INavigationNode {
  $type_RootNavigationNode: 1 = 1;
  private readonly _children: INavigationNode[] = [];
  readonly element: React.ReactNode;
  readonly id: string = "RootTabNode";
  readonly name: string;
  readonly parent: INavigationNode | undefined;
  readonly parentChain: INavigationNode[] = [];
  readonly showDetailLinks: boolean = true;

  get children() {
    return this._children;
  }

  constructor(name: string) {
    this.name = name;
  }

  addChild(node: INavigationNode) {
    this._children.push(node);
    node.parent = this;
  }

  removeChild(node: INavigationNode) {
    this._children.remove(node);
    node.parent = undefined;
  }

  equals(other: INavigationNode): boolean {
    return false;
  }
}

const isRootNavigationNode = (o: any): o is RootNavigationNode => o.$type_RootNavigationNode;