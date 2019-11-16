import React from "react";
import S from "./ScreenTabsArea.module.scss";

export const ScreenTabsArea: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);