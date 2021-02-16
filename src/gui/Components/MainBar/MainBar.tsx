import React from "react";
import S from "gui/Components/MainBar/MainBar.module.scss";
import cx from "classnames";

export const MainBar: React.FC<{ }> = props => (
  <div className={cx(S.root)}>
    {props.children}
  </div>
);
