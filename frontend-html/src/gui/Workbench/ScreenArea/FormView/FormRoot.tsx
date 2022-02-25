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

import S from "./FormRoot.module.scss";
import React from "react";
import { observer } from "mobx-react";
import { action } from "mobx";
import cx from "classnames";

@observer
export class FormRoot extends React.Component<{
  className?: string;
  style?: any
}> {
  componentDidMount() {
    window.addEventListener("click", this.handleWindowClick);
  }

  componentWillUnmount() {
    window.removeEventListener("click", this.handleWindowClick);
  }

  @action.bound handleWindowClick(event: any) {
    if (this.elmFormRoot && !this.elmFormRoot.contains(event.target)) {
    }
  }

  elmFormRoot: HTMLDivElement | null = null;
  refFormRoot = (elm: HTMLDivElement | null) => (this.elmFormRoot = elm);

  render() {
    return (
      <div
        ref={this.refFormRoot}
        className={cx(this.props.className, S.formRoot)}
        style={this.props.style}
      >
        {this.props.children}
      </div>
    );
  }
}
