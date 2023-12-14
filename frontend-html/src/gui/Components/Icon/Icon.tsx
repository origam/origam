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
import Svg from "react-inlinesvg";
import S from "./Icon.module.scss";
import cx from "classnames";

export class Icon extends React.Component<{
  src: string;
  className?: string;
  tooltip?: string;
  style?: {[key: string]: string};
  onClick?: () => void;
}> {
  render() {
    if (!this.props.src) {
      return null;
    }
    if (this.props.src.toLowerCase().endsWith("svg")) {
      return (
        <Svg
          onClick={() => this.props.onClick?.()}
          style={this.props.style}
          title={this.props.tooltip}
          src={this.props.src}
          className={cx(S.root, "icon", this.props.className)}
        />
      );
    }
    return (
      <img
        onClick={() => this.props.onClick?.()}
        style={this.props.style}
        title={this.props.tooltip}
        src={this.props.src}
        className={cx(S.root, "icon", this.props.className)}
        alt=""/>
    );
  }
}
