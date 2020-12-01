import React from "react";
import S from "./DataViewHeaderButton.module.scss";
import cx from "classnames";

export const DataViewHeaderButton: React.FC<{
  domRef?: any;
  isHidden?: boolean;
  disabled?: boolean;
  title?: string;
  onClick?(event: any): void;
}> = (props) => (
  <button
    title={props.title}
    ref={props.domRef}
    className={cx(props.disabled ? S.disabled : S.enabled, S.root, { hidden: props.isHidden })}
    onClick={props.onClick}
    disabled={props.disabled}
  >
    {props.children}
  </button>
);
