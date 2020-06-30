import React from "react";
import S from "./DataViewHeaderButton.module.scss";
import cx from "classnames";

export const DataViewHeaderButton: React.FC<{
  domRef?: any;
  isHidden?: boolean;
  onClick?(event: any): void;
}> = (props) => (
  <button
    ref={props.domRef}
    className={cx(S.root, { hidden: props.isHidden })}
    onClick={props.onClick}
  >
    {props.children}
  </button>
);
