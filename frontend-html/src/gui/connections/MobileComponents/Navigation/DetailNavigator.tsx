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

import React, { useState } from "react";
import S from "gui/connections/MobileComponents/Navigation/DetailNavigator.module.scss";
import SN from "gui/connections/MobileComponents/Navigation/NavigationButton.module.scss";
import { MobXProviderContext, observer } from "mobx-react";
import { Icon } from "@origam/components";
import { INavigationNode, NavigatorState } from "gui/connections/MobileComponents/Navigation/NavigationNode";
import cx from "classnames";
import { NavigationButton } from "gui/connections/MobileComponents/Navigation/NavigationButton";
import { MobileState } from "model/entities/MobileState";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";

@observer
export class StandaloneDetailNavigator extends React.Component<{
  node: INavigationNode;
}> {

  static contextType = MobXProviderContext;

  get mobileState(): MobileState {
    return this.context.application.mobileState;
  }

  navigatorState = new NavigatorState(this.mobileState, this.props.node);

  onScreenActivation(){
    this.mobileState.activeDataViewId = this.props.node.dataView?.id;
  }

  componentDidMount() {
    if(this.props.node.dataView){
      getOpenedScreen(this.props.node.dataView)
        .activationHandler
        .add(() => this.onScreenActivation());
      this.onScreenActivation();
    }
  }

  componentWillUnmount() {
    if(this.props.node.dataView){
      getOpenedScreen(this.props.node.dataView)
        .activationHandler
        .remove(() => this.onScreenActivation());
    }
  }

  render(){
    return <DetailNavigator node={this.navigatorState.currentNode} onNodeClick={node => this.navigatorState.onNodeClick(node)}/>
  }
}

@observer
export class DetailNavigator extends React.Component<{
  node: INavigationNode;
  onNodeClick: (node: INavigationNode) => void;
}> {

  static contextType = MobXProviderContext;

  get mobileState(): MobileState {
    return this.context.application.mobileState;
  }

  componentDidMount() {
    if(this.props.node.dataView?.isTableViewActive && !this.props.node.parent){
      this.mobileState.addDetailBreadCrumbNode(this.props.node.dataView);
    }
  }

  render() {
    if(!this.props.node){
      return <div/>;
    }
    return (
      <div className={S.root}>
        {this.props.node.element}
        {this.props.node.showDetailLinks() &&
          <NavigationButtonList
            onClick={(node) => this.props.onNodeClick(node)}
            nodes={this.props.node.children}
           />
        }
      </div>
    );
  }
}

export const NavigationButtonList: React.FC<{
  nodes: INavigationNode[];
  onClick: (node: INavigationNode) => void;
}> = observer((props) => {

  const [open, setOpen] = useState(false);

  if (props.nodes.length <= 3) {
    return (
      <div className={SN.navigationButtonContainer}>
        {props.nodes?.map(node =>
          <NavigationButton
            key={node.name}
            label={node.name}
            onClick={() => props.onClick(node)}
          />
        )}
      </div>
    );
  }
  return (
    <div
      className={cx(open ? S.navigatorButtonListRoot : "", S.navigationButtonContainer)}
      onClick={() => setOpen(!open)}
    >
      <div className={SN.navigationButton}>
        <div>
          {"Details"}
        </div>
        <Icon
          src={open ? "./icons/noun-chevron-933246.svg" : "./icons/noun-chevron-933254.svg"}
          className={SN.navigationIcon}
        />
      </div>
      <div className={S.navigationButtonList}>
        <div className={SN.navigationButtonContainer}>
          {open &&
            props.nodes.map(node =>
              <NavigationButton
                key={node.name}
                label={node.name}
                onClick={() => props.onClick(node)}
              />)
          }
        </div>
      </div>
    </div>
  );
});

