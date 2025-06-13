/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
import { observer } from "mobx-react";
import React from "react";
import S from "gui/connections/MenuComponents/EditButton.module.scss";
import CS from "gui/connections/MenuComponents/HeaderButton.module.scss";

@observer
export class EditButton extends React.Component<{
  isEnabled: boolean;
  isVisible: boolean;
  onClick: () => void;
  tooltip: string;
}> {

  getClass() {
    let className = `fas  fa-edit ${CS.headerIcon} ${S.editIcon}`;
    if (!this.props.isVisible && !this.props.isEnabled) {
      className += " " + CS.headerIconHidden;
    }
    if (this.props.isEnabled) {
      className += " " + CS.headerIconActive
    }
    return className;
  }

  render() {
    return (
      <i
        title={this.props.tooltip}
        className={this.getClass()}
        onClick={() => this.props.onClick()}
      />
    )
  }
}