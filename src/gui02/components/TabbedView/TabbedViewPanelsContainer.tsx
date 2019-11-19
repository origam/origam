import React from "react";
import S from "./TabbedViewPanelsContainer.module.scss";

export const TabbedViewPanelsContainer: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);