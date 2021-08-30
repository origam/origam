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

import S from "gui/Components/Form/FormField.module.scss";
import { inject, observer } from "mobx-react";
import { IDockType } from "model/entities/types/IProperty";
import { getRowStateDynamicLabel } from "model/selectors/RowState/getRowStateNameOverride";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import React from "react";
import { formatTooltipPlaintext } from "../ToolTip/FormatTooltipText";
import {FormViewEditor} from "gui/Workbench/ScreenArea/FormView/FormViewEditor";
import {observable} from "mobx";
export enum ICaptionPosition {
  Left = "Left",
  Right = "Right",
  Top = "Top",
  None = "None",
}

@inject(({ property }, { caption }) => {
  const rowId = getSelectedRowId(property);

  const ovrCaption = getRowStateDynamicLabel(property, rowId || "", property.id);

  return {
    caption: !!ovrCaption ? ovrCaption : caption,
  };
})
@observer
export class FormField extends React.Component<{
  caption: React.ReactNode;
  captionPosition?: ICaptionPosition;
  captionLength: number;
  dock?: IDockType;
  left: number;
  top: number;
  width: number;
  height: number;
  isCheckbox?: boolean;
  isHidden?: boolean;
  hideCaption?: boolean;
  captionColor?: string;
  toolTip?: string;
  xmlNode?: any;
  value?: any;
  textualValue?: any;
  isRichText: boolean;
  backgroundColor?: string;
}> {

  @observable
  toolTip: string | undefined | null;

  get captionStyle() {
    if (this.props.isHidden) {
      return {
        display: "none",
      };
    }
    switch (this.props.captionPosition) {
      default:
      case ICaptionPosition.Left:
        return {
          top: this.props.top,
          left: this.props.left - this.props.captionLength,
          color: this.props.captionColor,
        };
      case ICaptionPosition.Right:
        // 20 is expected checkbox width, might be needed to be set dynamically
        // if there is some difference in chekbox sizes between various platforms.
        return {
          top: this.props.top,
          left: this.props.isCheckbox ? this.props.left + 20 : this.props.left + this.props.width + 4,
          color: this.props.captionColor,
        };
      case ICaptionPosition.Top:
        return {
          top: this.props.top - 20, // TODO: Move this constant somewhere else...
          left: this.props.left,
          color: this.props.captionColor,
        };
    }
  }

  get formFieldStyle() {
    if (this.props.isHidden) {
      return {
        display: "none",
      };
    }
    if (this.props.dock === IDockType.Fill) {
      return {
        top: 0,
        left: 0,
        width: "100%",
        height: "100%",
      };
    }
    return {
      left: this.props.left,
      top: this.props.top,
      width: this.props.width,
      height: this.props.height,
    };
  }

  getToolTip() {
    if (this.props.toolTip) {
      return formatTooltipPlaintext(this.props.toolTip);
    } else {
      if(this.toolTip){
        return formatTooltipPlaintext(this.toolTip)
      }
      return undefined;
    }
  }

  render() {
    return (
      <>
        {this.props.captionPosition !== ICaptionPosition.None && !this.props.hideCaption &&
        <label
          className={S.caption}
          style={this.captionStyle}
          title={this.getToolTip()}
        >
          {this.props.caption}
        </label>
        }
        <div
          className={S.editor}
          style={this.formFieldStyle}
          title={this.getToolTip()}
        >
          <FormViewEditor
            value={this.props.value}
            isRichText={this.props.isRichText}
            textualValue={this.props.textualValue}
            xmlNode={this.props.xmlNode}
            backgroundColor={this.props.backgroundColor}
            onTextOverflowChanged={toolTip => this.toolTip = toolTip}
          />
        </div>
      </>
    );
  }
}
