import React from "react";
import S from "./ScreenContentArea.module.scss";

export const ScreenContentArea: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);