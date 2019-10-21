import React from "react";

export const Checkbox: React.FC<{
  checked?: boolean;
  indeterminate?: boolean;
  onChange?: (event: any) => void;
}> = props => (
  <input
    type="checkbox"
    checked={props.checked}
    onChange={props.onChange}
    ref={el => {
      if (el) {
        el.indeterminate = !!props.indeterminate;
      }
    }}
  />
);
