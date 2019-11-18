import React from "react";
import S from "./TabbedViewPanel.module.scss";
import cx from "classnames";

export const TabbedViewPanel: React.FC<{ isActive: boolean }> = props => (
  <div className={cx(S.root, { isActive: props.isActive })}>
    {props.children}
  </div>
);
