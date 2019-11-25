import React from "react";
import S from "./Fullscreen.module.scss";
import cx from "classnames";

export const Fullscreen: React.FC<{ isFullscreen?: boolean }> = props => (
  <div className={cx(S.root, { isFullscreen: props.isFullscreen })}>
    {props.children}
  </div>
);
