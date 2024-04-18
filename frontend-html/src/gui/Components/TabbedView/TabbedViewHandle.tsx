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
import S from "gui/Components/TabbedView/TabbedViewHandle.module.scss";
import cx from "classnames";
import { Icon } from "gui/Components/Icon/Icon";

export class TabbedViewHandle extends React.Component<{
  title?: string;
  id?: string;
  isActive?: boolean;
  hasCloseBtn?: boolean;
  isDirty?: boolean;
  onClick?(event: any): void;
  onCloseClick?(event: any): void;
  onCloseMouseDown?(event: any): void;
}> {
  render() {
    return (
      <div
        onClick={this.props.onClick}
        className={cx(S.root, {isActive: this.props.isActive, isDirty: this.props.isDirty})}
        title={this.props.title}
        id={this.props.id}
      >
        <div className={S.label}>{this.props.children}</div>
        {this.props.hasCloseBtn && (
          <a
            className={S.closeBtn + " tabHandle"}
            onClick={this.props.onCloseClick}
            onMouseDown={this.props.onCloseMouseDown}
          >
            <Icon src="./icons/close.svg" tooltip={""}/>
          </a>
        )}
      </div>
    );
  }
}
