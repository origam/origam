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

import React, { RefObject } from "react";
import S from "gui/Components/WorkQueues/WorkQueuesItem.module.scss";
import cx from "classnames";
import { MobXProviderContext } from "mobx-react";
import { getMainMenuState } from "model/selectors/MainMenu/getMainMenuState";

export class WorkQueuesItem extends React.Component<{
  isActiveScreen?: boolean;
  isOpenedScreen?: boolean;
  isHidden?: boolean;
  isEmphasized?: boolean;
  level?: number;
  icon?: React.ReactNode;
  label?: React.ReactNode;
  tooltip?: string;
  id?: string;
  onClick?(event: any): void;
}> {
  static contextType = MobXProviderContext;
  itemRef: RefObject<HTMLAnchorElement> = React.createRef();

  componentDidMount() {
    if (this.props.id) {
      this.mainMenuState.setReference(this.props.id, this.itemRef);
    }
  }

  get mainMenuState() {
    return getMainMenuState(this.context.application);
  }

  render() {
    return (
      <a
        ref={this.itemRef}
        className={cx(
          S.root,
          {
            isActiveScreen: this.props.isActiveScreen,
            isOpenedScreen: this.props.isOpenedScreen,
          },
          {isHidden: this.props.isHidden},
          {isEmphasized: this.props.isEmphasized}
        )}
        style={{paddingLeft: `${(this.props.level || 1) * 1.6667}em`}}
        onClick={this.props.onClick}
        title={this.props.tooltip}
      >
        <div className={S.icon}>{this.props.icon}</div>
        <div className={S.label}>{this.props.label}</div>
      </a>
    );
  }
}
