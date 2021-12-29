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
import { MobXProviderContext, observer } from "mobx-react";
import { MobileState } from "model/entities/MobileState";


@observer
export class TabNavigator extends React.Component<{
  name: string;
  nodes: TabNavigationNode[];
}> {

  static contextType = MobXProviderContext;

  get mobileState(): MobileState {
    return this.context.application.mobileState;
  }

  componentDidMount() {
    this.mobileState.node = new RootNavigationNode({name: this.props.name, children: this.props.nodes});
  }

  render() {
    if(!this.mobileState.node){
      return null;
    }
    if (isRootNavigationNode(this.mobileState.node)) {
      return (
        <div className={SN.navigationButtonContainer}>
          {this.props.nodes.map(node =>
            <NavigationButton
              key={node.name}
              label={node.name}
              onClick={() => {
                return this.mobileState.node = node
              }
              }
            />)
          }
        </div>
      );
    } else {
      return <DetailNavigator/>
    }
  }
}

class RootNavigationNode implements INavigationNode {
  $type_RootNavigationNode: 1 = 1;
  readonly children: INavigationNode[];
  readonly element: React.ReactNode;
  readonly id: string = "RootTabNode";
  readonly name: string;
  readonly parent: INavigationNode | undefined;
  readonly parentChain: INavigationNode[] = [];
  readonly showDetailLinks: boolean = true;


  constructor(args:{name: string, children: TabNavigationNode[]}) {
    this.name = args.name;
    for (const child of args.children) {
      child.parent = this;
    }
    this.children = args.children;
  }

  equals(other: INavigationNode): boolean {
    return false;
  }
}

const isRootNavigationNode = (o: any): o is RootNavigationNode => o.$type_RootNavigationNode;