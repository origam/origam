import React from "react";
import S from "./MainBar.module.scss";

export const MainBar: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);