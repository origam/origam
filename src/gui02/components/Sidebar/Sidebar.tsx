import S from "./Sidebar.module.scss";
import React from "react";

export const Sidebar: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);
