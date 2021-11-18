import React from "react";
import cx from "classnames";
import S from "gui/Components/DataViewHeader/DataViewHeaderGroup.module.scss";

export const DataViewHeaderGroup: React.FC<{
  isHidden?: boolean;
  noShrink?: boolean;
  noDivider?: boolean;
  domRef?: any;
  grovable?: boolean;
}> = (props) => (
  <div
    className={cx(S.root, {
      isHidden: props.isHidden,
      noDivider: props.noDivider,
      grovable: props.grovable,
      noShrink: props.noShrink,
    })}
    ref={props.domRef}
  >
    {props.children}
  </div>
);
