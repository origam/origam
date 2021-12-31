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
import { INavigationNode } from "gui/connections/MobileComponents/Navigation/NavigationNode";
import { NavigationButton } from "gui/connections/MobileComponents/Navigation/NavigationButton";
import { DetailNavigator } from "gui/connections/MobileComponents/Navigation/DetailNavigator";
import { MobXProviderContext, observer } from "mobx-react";
import { observable } from "mobx";
import { MobileState } from "model/entities/MobileState";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";

@observer
export class TabNavigator extends React.Component<{
  rootNode: INavigationNode;
}> {

  static contextType = MobXProviderContext;

  get mobileState(): MobileState {
    return this.context.application.mobileState;
  }

  @observable
  currentNode: INavigationNode = this.props.rootNode;

  onNodeClick(node: INavigationNode){
    this.currentNode = node;
    this.mobileState.activeDataViewId = node.dataView?.id;
  }

  onScreenActivation(){
    this.mobileState.activeDataViewId = this.props.rootNode.dataView?.id
  }

  componentDidMount() {
    if(this.props.rootNode.dataView){
      getOpenedScreen(this.props.rootNode.dataView)
        .activationHandler
        .add(() => this.onScreenActivation());
      this.onScreenActivation();
    }
  }

  componentWillUnmount() {
    if(this.props.rootNode.dataView){
      getOpenedScreen(this.props.rootNode.dataView)
        .activationHandler
        .remove(() => this.onScreenActivation());
    }
  }

  render() {
    if(!this.currentNode){
      return null;
    }
    if (!this.currentNode.parent) {
      return (
        <div className={SN.navigationButtonContainer}>
          {this.currentNode.children.map(node =>
            <NavigationButton
              key={node.id}
              label={node.name}
              onClick={() => this.onNodeClick(node)}
            />)
          }
        </div>
      );
    } else {
      return <DetailNavigator node={this.currentNode} onNodeClick={node => this.currentNode = node}/>
    }
  }
}
