import React from "react";
import S from "gui/Components/TabbedView/TabbedViewPanelsContainer.module.scss";

export const TabbedViewPanelsContainer: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);