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

import cx from "classnames";

import S from "gui/Components/MainMenu/MainMenuItem.module.scss";

export class MainMenuItem extends React.Component<{
  icon: React.ReactNode;
  label: string;
  level: number;
  isActive: boolean;
  isHidden: boolean;
  isHighLighted?: boolean;
  id?: string;
  isOpenedScreen?: boolean;
  isActiveScreen?: boolean;

  onClick?(event: any): void;
  onContextMenu?(event: any): void;
  refDom?: any;
}> {
  render() {
    return (
      <div className={S.linkContainer} onContextMenu={this.props.onContextMenu}>
        <a
          ref={this.props.refDom}
          className={cx(S.anchor, {
            isActive: this.props.isActive,
            isHidden: this.props.isHidden,
            isOpenedScreen: this.props.isOpenedScreen,
            isActiveScreen: this.props.isActiveScreen,
            isHighLighted: this.props.isHighLighted,
          })}
          style={{paddingLeft: `${this.props.level * 1.6667}em`}}
          onClick={this.props.onClick}
          title={this.props.label}
        >
          <div className={S.icon}>{this.props.icon}</div>
          <div className={S.label} id={this.props.id}>{this.props.label}</div>
        </a>
      </div>
    );
  }
}
