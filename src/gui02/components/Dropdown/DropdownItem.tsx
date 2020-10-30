import React from "react";
import S from "./DropdownItem.module.scss";
import cx from "classnames";

export const DropdownItem: React.FC<{
  onClick?(event: any): void;
  isDisabled?: boolean;
  isSelected?: boolean;
}> = props => {
  function getStyle(){
    if(props.isDisabled){
      return "isDisabled"
    }
    return props.isSelected ? S.isSelected : ""
  }

  return <div
    onClick={props.onClick}
    className={cx(S.root, getStyle())}
  >
    {props.children}
  </div>
};
