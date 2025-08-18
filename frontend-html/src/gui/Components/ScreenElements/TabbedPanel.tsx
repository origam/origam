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
import S from "./TabbedPanel.module.scss";
import { observer } from "mobx-react";

@observer
export class TabBody extends React.Component<{ isActive: boolean }> {
  render() {
    return (
      <div className={S.tabBody + (!this.props.isActive ? " hidden" : "")}>
        {this.props.children}
      </div>
    );
  }
}

@observer
export class TabHandle extends React.Component<{
  isActive: boolean;
  label: string;
  onClick: (event: any) => void;
}> {
  render() {
    return (
      <div
        className={S.tabHandle + (this.props.isActive ? ` active` : "")}
        onClick={this.props.onClick}
      >
        {this.props.label}
      </div>
    );
  }
}

@observer
export class TabbedPanel extends React.Component<{
  handles: React.ReactNode;
}> {
  render() {
    return (
      <div className={S.tabbedPanel}>
        <div className={S.tabHandles}>{this.props.handles}</div>
        {this.props.children}
      </div>
    );
  }
}
