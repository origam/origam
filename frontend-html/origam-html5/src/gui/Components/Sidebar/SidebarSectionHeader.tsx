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
import S from "gui/Components/Sidebar/SidebarSectionHeader.module.scss";

export class SidebarSectionHeader extends React.Component<{
  icon: React.ReactNode;
  label: React.ReactNode;
  isActive: boolean;
  onClick?(event: any): void;
  refDom?: any;
  onContextMenu?: (event: any) => void;
  id?: string;
}> {
  render() {
    return (
      <a
        id={this.props.id}
        ref={this.props.refDom}
        className={cx(S.root, {
          isActive: this.props.isActive
        })}
        onClick={this.props.onClick}
        onContextMenu={this.props.onContextMenu}
      >
        <div className={S.icon}>{this.props.icon}</div>
        <div className={S.label}>{this.props.label}</div>
      </a>
    );
  }
}
