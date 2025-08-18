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
import { IDockType, IProperty } from "model/entities/types/IProperty";
import { getRowStateDynamicLabel } from "model/selectors/RowState/getRowStateNameOverride";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import React from "react";
import { formatTooltipPlaintext } from "../ToolTip/FormatTooltipText";
import { FormViewEditor } from "gui/Workbench/ScreenArea/FormView/FormViewEditor";
import { observable } from "mobx";
import { FieldDimensions } from "gui/Components/Form/FieldDimensions";
import { getFieldErrorMessage } from "model/selectors/DataView/getFieldErrorMessage";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";

export enum ICaptionPosition {
  Left = "Left",
  Right = "Right",
  Top = "Top",
  None = "None",
}

export interface IFormFieldProps {
  caption: React.ReactNode;
  captionPosition?: ICaptionPosition;
  captionLength: number;
  dock?: IDockType;
  fieldDimensions: FieldDimensions;
  isCheckbox?: boolean;
  isHidden?: boolean;
  hideCaption?: boolean;
  captionColor?: string;
  tooltip?: string;
  xmlNode?: any;
  value?: any;
  textualValue?: any;
  isRichText: boolean;
  backgroundColor?: string;
  property?: IProperty;
}

@inject(({property}, {caption}) => {
  const rowId = getSelectedRowId(property);

  const ovrCaption = getRowStateDynamicLabel(property, rowId || "", property.id);

  return {
    caption: !!ovrCaption ? ovrCaption : caption,
    property: property
  };
})
@observer
export class FormField extends React.Component<IFormFieldProps> {

  @observable
  dynamicTooltip: string | undefined | null;

  render() {
    const row = getSelectedRow(this.props.property);
    const invalidMessage = row
      ? getFieldErrorMessage(this.props.property!)(row, this.props.property!)
      : undefined;

    return (
      <>
        {this.props.captionPosition !== ICaptionPosition.None && !this.props.hideCaption && this.props.dock !== IDockType.Fill &&
        <label
          className={S.caption}
          style={getCaptionStyle(this.props)}
          title={getTooltip(this.props.tooltip, this.dynamicTooltip)}
        >
          {this.props.caption}
        </label>
        }
        <div
          className={S.editor}
          style={getFormFieldStyle(this.props)}
          title={getTooltip(this.props.tooltip, this.dynamicTooltip)}
        >
          <FormViewEditor
            value={this.props.value}
            isRichText={this.props.isRichText}
            textualValue={this.props.textualValue}
            xmlNode={this.props.xmlNode}
            backgroundColor={this.props.backgroundColor}
            onTextOverflowChanged={tooltip => this.dynamicTooltip = tooltip}
            dock={this.props.dock}
          />
          {invalidMessage && (
            <div className={S.notification} title={invalidMessage}>
              <i className="fas fa-exclamation-circle red"/>
            </div>
          )}
        </div>
      </>
    );
  }
}

export function getCaptionStyle(props: IFormFieldProps) {
  const dimensions = props.fieldDimensions;
  const style = {...(props.property?.style ?? {})};
  if(dimensions.isUnset){
    return {...dimensions.asStyle(), ...style};
  }
  if (props.isHidden) {
    style["display"] = "none";
    return style;
  }
  switch (props.captionPosition) {
    default:
    case ICaptionPosition.Left:
      style["top"] = dimensions.top;
      style["left"] = dimensions.left! - props.captionLength;
      style["color"] = props.captionColor;
      return style;
    case ICaptionPosition.Right:
      // 20 is expected checkbox width, might be needed to be set dynamically
      // if there is some difference in chekbox sizes between various platforms.
      style["top"] = dimensions.top;
      style["left"] = props.isCheckbox ? dimensions.left! + 20 : dimensions.left! + dimensions.width! + 4;
      style["color"] = props.captionColor;
      return style;
    case ICaptionPosition.Top:
      style["top"] = dimensions.top! - 20; // TODO: Move this constant somewhere else...
      style["left"] = dimensions.left;
      style["color"] = props.captionColor
      return style;
  }
}

export function getFormFieldStyle(props: IFormFieldProps) {
  const dimensions = props.fieldDimensions;
  if(dimensions.isUnset){
    return dimensions.asStyle();
  }
  if (props.isHidden) {
    return {
      display: "none",
    };
  }
  if (props.dock === IDockType.Fill) {
    return {
      top: 0,
      left: 0,
      width: "100%",
      height: "100%",
    };
  }
  return {
    left: dimensions.left,
    top: dimensions.top,
    width: dimensions.width,
    height: dimensions.height,
  };
}

export function getTooltip(propertyTooltip: string | undefined, additionalText: string | undefined | null = "" ){
  let finalTooltip = propertyTooltip ?? "";
  if (additionalText) {
    finalTooltip = additionalText + "\n\n" + finalTooltip;
  }
  return formatTooltipPlaintext(finalTooltip);
}


