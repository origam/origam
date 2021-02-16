import React from "react";

export const DataViewHeaderDropDownItem: React.FC<{
  onClick?(event: any): void;
}> = (props) => (
  <div onClick={props.onClick}>
    {props.children}
  </div>
);