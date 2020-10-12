import React, { useState, useEffect } from "react";
import { IProperty } from "model/entities/types/IProperty";
import { BoolEditor } from "gui/Components/ScreenElements/Editors/BoolEditor";
import S from "./CheckBox.module.scss";
import { inject } from "mobx-react";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { onFieldBlur } from "model/actions-ui/DataView/TableView/onFieldBlur";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import {IFocusable} from "model/entities/FocusManager";

export const CheckBox: React.FC<{
  checked: boolean;
  readOnly: boolean;
  isHidden?: boolean;
  onChange?: (event: any, value: any) => void;
  property?: IProperty;
  onKeyDown: (event: any) => void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
}> = inject(({ property, formPanelView }) => {
  const row = getSelectedRow(formPanelView)!;
  return {
    property,
    onEditorBlur: (event: any) => onFieldBlur(formPanelView)(event),
    onChange: (event: any, value: any) => onFieldChange(formPanelView)(event, row, property, value),
  };
})((props) => {
  const [isFocused, setIsFocused] = useState<boolean>(false);

  const label = props.property!.name;
  const height = props.property!.height;
  const width = props.property!.width;
  const left = props.property!.x;
  const top = props.property!.y;

  function captionStyle() {
    if (props.isHidden) {
      return {
        display: "none",
      };
    }
    // 20 is expected checkbox width, might be needed to be set dynamically
    // if there is some difference in chekbox sizes between various platforms.
    return {
      top: top,
      left: left + 20,
    };
  }

  function formFieldStyle() {
    if (props.isHidden) {
      return {
        display: "none",
      };
    }
    return {
      left: left,
      top: top,
      width: width,
      height: height,
    };
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
    <div>
      <div className={S.editor} style={formFieldStyle()}>
        <BoolEditor
          id={props.property!.modelInstanceId}
          value={props.checked}
          isInvalid={false}
          isReadOnly={props.readOnly}
          onBlur={onInputBlur}
          onFocus={onInputFocus}
          onChange={onChange}
          onKeyDown={event => props.onKeyDown(event)}
          subscribeToFocusManager={props.subscribeToFocusManager}
        />
      </div>
      <label
        htmlFor={props.property!.modelInstanceId}
        className={S.caption + " " + (isFocused ? S.focusedLabel : S.unFocusedLabel)}
        style={captionStyle()}
      >
        {label}
      </label>
    </div>
  );
});
