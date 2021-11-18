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

import * as React from "react";
import * as ReactDOM from "react-dom";
import { IPositionedFieldProps } from "./types";
import { observer } from "mobx-react";
import S from "./PositionedField.module.css";
import cx from "classnames";

@observer
export class PositionedField extends React.Component<IPositionedFieldProps> {
  render() {
    const {columnIndex, worldBounds, cellRectangle} = this.props;
    const columnLeft = cellRectangle.columnLeft;
    const columnWidth = cellRectangle.columnWidth;
    const rowTop = cellRectangle.rowTop;
    const rowHeight = 25; //cellRectangle.rowHeight;
    const {scrollTop, scrollLeft} = this.props.scrollOffsetSource;
    return ReactDOM.createPortal(
      <div
        className={cx(S.positionedField, {isFirstColumn: columnIndex === 0})}
        onClick={(event: any) => event.stopPropagation()}
        onMouseEnter={this.props.onMouseEnter}
        style={{
          top: worldBounds.top + rowTop - scrollTop,
          left:
            columnIndex < this.props.fixedColumnsCount
              ? worldBounds.left + columnLeft
              : worldBounds.left + columnLeft - scrollLeft,
          width: columnWidth,
          height: rowHeight,
        }}
      >
        {this.props.children}
      </div>,
      document.getElementById("form-field-portal")!
    );
  }
}
