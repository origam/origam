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
import { FieldDimensions } from "gui/Components/Form/FieldDimensions";
import { IProperty } from "model/entities/types/IProperty";
import { inject } from "mobx-react";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { onFieldBlur } from "model/actions-ui/DataView/TableView/onFieldBlur";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import { MobileBooleanInput } from "gui/connections/MobileComponents/Form/MobileBooleanInput";

export const MobileCheckBox: React.FC<{
  isHidden?: boolean;
  checked: boolean;
  readOnly: boolean;
  onChange?: (event: any, value: any) => void;
  labelColor?: string;
  property?: IProperty;
}> = inject(({property, formPanelView}) => {
  const row = getSelectedRow(formPanelView)!;
  return {
    property,
    onEditorBlur: (event: any) => onFieldBlur(formPanelView)(),
    onChange: (event: any, value: any) => onFieldChange(formPanelView)({
      event: event,
      row: row,
      property: property,
      value: value,
    }),
  };
})((props) => {
  const label = props.property!.name;
  const fieldDimensions = new FieldDimensions();

  function captionStyle() {
    if (props.isHidden) {
      return {
        display: "none",
      };
    }
    const style = fieldDimensions.asStyle();
    style["color"] = props.labelColor;
    return style;
  }

  function formFieldStyle() {
    if (props.isHidden) {
      return {
        display: "none",
      };
    }
    return fieldDimensions.asStyle();
  }

  return (
    <div>
      <label
        style={captionStyle()}
      >
        {label}
      </label>
      <div style={formFieldStyle()}>
        <MobileBooleanInput
          checked={props.checked}
          disabled={props.readOnly}
          onChange={event => props.onChange?.(event, event.target.checked)}
        />
      </div>
    </div>
  );
});

