import S from "gui/Components/Sidebar/Sidebar.module.scss";
import React from "react";

export const Sidebar: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);
