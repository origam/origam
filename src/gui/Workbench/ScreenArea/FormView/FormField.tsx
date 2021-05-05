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

import S from "./FormField.module.css";
import React from "react";
import {inject, observer} from "mobx-react";
import {getSelectedRowId} from "model/selectors/TablePanelView/getSelectedRowId";
import {getRowStateAllowRead} from "model/selectors/RowState/getRowStateAllowRead";

export enum ICaptionPosition {
  Left = "Left",
  Right = "Right",
  Top = "Top",
  None = "None"
}

@inject(({ property }) => {
  const rowId = getSelectedRowId(property);
  const isHidden = !getRowStateAllowRead(property, rowId || "", property.id);
  return {
    isHidden
  };
})
@observer
export class FormField extends React.Component<{
  Id: string;
  Name: string;
  CaptionLength: number;
  CaptionPosition: ICaptionPosition | undefined;
  Column: string;
  Entity: string;
  Height: number;
  Width: number;
  Y: number;
  X: number;
  isHidden?: boolean;
}> {
  fieldNameStyle() {
    // TODO: !!! Proper typing of props (numbers instead of strings etc...)

    switch (this.props.CaptionPosition) {
      case ICaptionPosition.Left:
        return {
          top: this.props.Y,
          left: this.props.X - this.props.CaptionLength,
          width: this.props.CaptionLength
          //  height: this.props.height
        };
      case ICaptionPosition.Right:
        return {
          top: this.props.Y,
          left:
            this.props.Column === "CheckBox"
              ? this.props.X + this.props.Height
              : this.props.X + this.props.Width,
          width: this.props.CaptionLength
          //  height: this.props.height
        };
      case ICaptionPosition.Top:
        return {
          top: this.props.Y - 20,
          left: this.props.X,
          width: this.props.CaptionLength
        };
      default:
        return {};
    }
  }

  render() {
    const isHidden = this.props.isHidden;
    const display = isHidden ? "none" : undefined;
    return (
      <>
        {this.props.CaptionPosition !== ICaptionPosition.None && (
          <div
            className={S.formFieldName}
            style={{ ...this.fieldNameStyle(), display }}
          >
            {this.props.Name}
          </div>
        )}
        <div
          className={S.formField}
          style={{
            top: this.props.Y,
            left: this.props.X,
            width: this.props.Width,
            height: this.props.Height,
            display
          }}
          onClick={(event: any) => event.stopPropagation()}
        >
          {this.props.children ? (
            this.props.children
          ) : (
            <div className={S.unknownEditor}>{this.props.Id}</div>
          )}
        </div>
      </>
    );
  }
}
