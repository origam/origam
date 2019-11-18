import React from "react";
import cx from "classnames";
import S from "./DataViewHeaderButtonGroup.module.scss";

export const DataViewHeaderButtonGroup: React.FC<{
  isHidden?: boolean;
  domRef?: any;
}> = props => (
  <div ref={props.domRef} className={cx(S.root, { isHidden: props.isHidden })}>
    {props.children}
  </div>
);
