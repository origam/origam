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

import React, { useMemo, useState } from "react";
import { IProperty } from "model/entities/types/IProperty";
import { BoolEditor } from "gui/Components/ScreenElements/Editors/BoolEditor";
import S from "gui/Components/Form/CheckBox.module.scss";
import { inject } from "mobx-react";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { onFieldBlur } from "model/actions-ui/DataView/TableView/onFieldBlur";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import { IFocusable } from "model/entities/FormFocusManager";
import { FieldDimensions } from "gui/Components/Form/FieldDimensions";
import * as uuid from 'uuid';

export const CheckBox: React.FC<{
  checked: boolean;
  readOnly: boolean;
  isHidden?: boolean;
  onChange?: (event: any, value: any) => void;
  property?: IProperty;
  onKeyDown: (event: any) => void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  onClick: () => void;
  labelColor?: string;
  fieldDimensions: FieldDimensions;
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
  const labelId = useMemo(() => uuid.v4(), []);
  const [isFocused, setIsFocused] = useState<boolean>(false);

  const label = props.property!.name;

  function captionStyle() {
    if (props.isHidden) {
      return {
        display: "none",
      };
    }
    return {
      color: props.labelColor
    };
  }

  function formFieldStyle() {
    if (props.isHidden) {
      return {
        display: "none",
      };
    }
    return props.fieldDimensions.asStyle();
  }

  function onChange(event: any, state: boolean) {
    if (!props.readOnly) props.onChange?.(event, state);
  }

  function onInputFocus() {
    setIsFocused(true);
  }

  function onInputBlur() {
    setIsFocused(false);
  }

  return (
    <div className={S.root} style={formFieldStyle()}>
      <BoolEditor
        id={labelId}
        value={props.checked}
        isReadOnly={props.readOnly}
        onBlur={onInputBlur}
        onFocus={onInputFocus}
        onChange={onChange}
        onKeyDown={event => props.onKeyDown(event)}
        subscribeToFocusManager={props.subscribeToFocusManager}
        onClick={props.onClick}
      />
      <label
        htmlFor={labelId}
        className={S.caption + " " + (isFocused ? S.focusedLabel : S.unFocusedLabel)}
        style={captionStyle()}
      >
        {label}
      </label>
    </div>
  );
});
