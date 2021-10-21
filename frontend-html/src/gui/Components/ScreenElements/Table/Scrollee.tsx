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

import { observer } from "mobx-react";
import * as React from "react";
import { IScrolleeProps } from "./types";
import S from "./Scrollee.module.css";

/*
  Component translating its content according to scrollOffsetSource.
*/

// TODO: Maybe add hideOverflow property to disable content clipping? (or allow some custom class?)
@observer
export default class Scrollee extends React.Component<IScrolleeProps> {
  public render() {
    return (
      <div
        className={S.scrollee}
        style={{
          width: this.props.width,
          height: this.props.height,
          zIndex: this.props.zIndex || 0,
        }}
      >
        <div
          className={S.scrolleeShifted}
          style={{
            top:
              (this.props.fixedVert ? 0 : -this.props.scrollOffsetSource.scrollTop) +
              (this.props.offsetTop || 0),
            left:
              (this.props.fixedHoriz ? 0 : -this.props.scrollOffsetSource.scrollLeft) +
              (this.props.offsetLeft || 0),
          }}
        >
          {this.props.children}
        </div>
      </div>
    );
  }
}
