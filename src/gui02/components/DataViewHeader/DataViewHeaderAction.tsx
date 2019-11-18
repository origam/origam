import React from "react";
import S from "./DataViewHeaderAction.module.scss";
import cx from "classnames";

export const DataViewHeaderAction: React.FC<{
  onClick?(event: any): void;
  className?: string;
  isActive?: boolean;
  refDom?: any;
}> = props => (
  <div
    className={cx(S.root, props.className, { isActive: props.isActive })}
    onClick={props.onClick}
    ref={props.refDom}
  >
    {props.children}
  </div>
);
