import React from "react";

export const Checkbox: React.FC<{
  checked?: boolean;
  indeterminate?: boolean;
  onChange?: (event: any) => void;
  onClick?: (event: any) => void;
  onClickCapture?: (event: any) => void;
}> = props => (
  <input
    type="checkbox"
    checked={props.checked}
    onChange={props.onChange}
    onClick={props.onClick}
    onClickCapture={props.onClickCapture}
    ref={el => {
      if (el) {
        el.indeterminate = !!props.indeterminate;
      }
    }}
  />
);
