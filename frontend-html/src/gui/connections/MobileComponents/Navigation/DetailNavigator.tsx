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

import React, { createContext } from "react";
import S from "gui/connections/MobileComponents/Navigation/DetailNavigator.module.scss";
import { MobXProviderContext, observer } from "mobx-react";
import { INavigationNode, NavigatorState } from "gui/connections/MobileComponents/Navigation/NavigationNode";
import { MobileState } from "model/entities/MobileState/MobileState";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";
import { BreadCrumbsState } from "model/entities/MobileState/BreadCrumbsState";
import SN from "gui/connections/MobileComponents/Navigation/NavigationButton.module.scss";
import { NavigationButton } from "gui/connections/MobileComponents/Navigation/NavigationButton";


@observer
export class StandaloneDetailNavigator extends React.Component<{
  node: INavigationNode;
}> {

  static contextType = MobXProviderContext;

  get mobileState(): MobileState {
    return this.context.application.mobileState;
  }

  navigatorState = new NavigatorState(this.mobileState, this.props.node);

  onScreenActivation() {
    this.mobileState.activeDataViewId = this.props.node.dataView?.id;
  }

  componentDidMount() {
    if (this.props.node.dataView) {
      getOpenedScreen(this.props.node.dataView)
        .activationHandler
        .set(() => this.onScreenActivation());
      this.onScreenActivation();
    }
  }

  componentWillUnmount() {
    if (this.props.node.dataView) {
      getOpenedScreen(this.props.node.dataView)
        .activationHandler
        .clear();
    }
  }

  render() {
    return <DetailNavigator
      node={this.navigatorState.currentNode}
      onNodeClick={node => this.navigatorState.onLinkClick(node)}
    />
  }
}

class ExtraButtons {
  constructor(
    public node: INavigationNode,
    public onNodeClick: (node: INavigationNode) => void
  ) {
  }
}

export const ExtraButtonsContext = createContext<ExtraButtons | null>(null);

@observer
export class DetailNavigator extends React.Component<{
  node: INavigationNode;
  onNodeClick: (node: INavigationNode) => void;
}> {

  static contextType = MobXProviderContext;

  get breadCrumbsState(): BreadCrumbsState {
    return this.context.application.mobileState.breadCrumbsState;
  }

  componentDidMount() {
    if (this.props.node.dataView?.isTableViewActive && !this.props.node.parent) {
      if(this.props.node.element){
        this.breadCrumbsState.addDetailBreadCrumbNodeToRoot(this.props.node.dataView);
      }else{
        this.breadCrumbsState.removeDetailNode();
      }
    }
  }

  renderElement() {
    return (
      <ExtraButtonsContext.Provider value={new ExtraButtons(this.props.node, this.props.onNodeClick)}>
        {this.props.node.element}
      </ExtraButtonsContext.Provider>
    );
  }

  renderNavigationButtonList() {
    return (
      <div className={SN.navigationButtonContainer}>
        {this.props.node.children.map(node =>
          <NavigationButton
            key={node.name}
            label={node.name}
            onClick={() => this.props.onNodeClick(node)}
          />)}
      </div>
    );
  }

  render() {
    if (!this.props.node) {
      return <div/>;
    }
    return (
      <div className={S.root}>
        {this.props.node.element
          ? this.renderElement()
          : this.renderNavigationButtonList()
        }
      </div>
    );
  }
}

