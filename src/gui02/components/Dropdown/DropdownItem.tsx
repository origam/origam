import React from "react";
import S from "./DropdownItem.module.scss";
import cx from "classnames";

export const DropdownItem: React.FC<{
  onClick?(event: any): void;
  isDisabled?: boolean;
}> = props => (
  <div
    onClick={props.onClick}
    className={cx(S.root, { isDisabled: props.isDisabled })}
  >
    {props.children}
  </div>
);
