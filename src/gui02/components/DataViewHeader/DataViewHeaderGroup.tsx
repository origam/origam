import React from "react";
import cx from "classnames";
import S from "./DataViewHeaderGroup.module.scss";

export const DataViewHeaderGroup: React.FC<{
  isHidden?: boolean;
  domRef?: any;
}> = props => (
  <div className={cx(S.root, { isHidden: props.isHidden })} ref={props.domRef}>
    {props.children}
  </div>
);
