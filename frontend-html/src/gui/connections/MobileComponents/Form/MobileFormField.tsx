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
import "gui/connections/MobileComponents/Form/MobileForm.scss";
import {
  getCaptionStyle, getFormFieldStyle,
  getTooltip,
  ICaptionPosition,
  IFormFieldProps
} from "gui/Components/Form/FormField";
import { inject, observer } from "mobx-react";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getRowStateDynamicLabel } from "model/selectors/RowState/getRowStateNameOverride";
import { observable } from "mobx";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getFieldErrorMessage } from "model/selectors/DataView/getFieldErrorMessage";
import S from "gui/Components/Form/FormField.module.scss";
import { MobileFormViewEditor } from "gui/connections/MobileComponents/Form/MobileFormViewEditor";

@inject(({property}, {caption}) => {
  const rowId = getSelectedRowId(property);

  const ovrCaption = getRowStateDynamicLabel(property, rowId || "", property.id);

  return {
    caption: !!ovrCaption ? ovrCaption : caption,
    property: property
  };
})
@observer
export class MobileFormField extends React.Component<IFormFieldProps> {

  @observable
  dynamicTooltip: string | undefined | null;

  render() {
    const row = getSelectedRow(this.props.property);
    const invalidMessage = row
      ? getFieldErrorMessage(this.props.property!)(row, this.props.property!)
      : undefined;

    return (
      <div className={"formItem"}>
        {this.props.captionPosition !== ICaptionPosition.None && !this.props.hideCaption &&
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
          <MobileFormViewEditor
            value={this.props.value}
            isRichText={this.props.isRichText}
            textualValue={this.props.textualValue}
            xmlNode={this.props.xmlNode}
            backgroundColor={this.props.backgroundColor}
            onTextOverflowChanged={tooltip => this.dynamicTooltip = tooltip}
            property={this.props.property!}
          />
          {invalidMessage && (
            <div className={S.notification} title={invalidMessage}>
              <i className="fas fa-exclamation-circle red"/>
            </div>
          )}
        </div>
      </div>
    );
  }
}



