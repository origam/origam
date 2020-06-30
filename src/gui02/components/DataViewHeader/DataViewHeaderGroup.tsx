import React from "react";
import cx from "classnames";
import S from "./DataViewHeaderGroup.module.scss";

export const DataViewHeaderGroup: React.FC<{
  isHidden?: boolean;
  domRef?: any;
  grovable?: boolean;
}> = (props) => (
  <div
    className={cx(S.root, { isHidden: props.isHidden, grovable: props.grovable })}
    ref={props.domRef}
  >
    {props.children}
  </div>
);
