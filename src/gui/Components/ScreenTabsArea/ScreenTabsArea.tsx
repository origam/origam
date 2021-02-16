import React from "react";
import S from "gui/Components/ScreenTabsArea/ScreenTabsArea.module.scss";

export const ScreenTabsArea: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);