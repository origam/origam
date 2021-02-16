import React from "react";
import S from "gui/Components/TabbedView/TabbedView.module.scss";

export const TabbedView: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);
