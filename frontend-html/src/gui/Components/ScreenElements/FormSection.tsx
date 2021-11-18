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

import S from "./FormSection.module.css";
import React from "react";

export class FormSection extends React.Component<{
  width: number;
  height: number;
  x: number;
  y: number;
  title: string;
}> {
  render() {
    return (
      <div
        className={S.formSection}
        style={{
          top: this.props.y,
          left: this.props.x,
          width: this.props.width,
          height: this.props.height
        }}
      >
        <div className={S.sectionTitle}>{this.props.title}</div>
        {this.props.children}
      </div>
    );
  }
}
