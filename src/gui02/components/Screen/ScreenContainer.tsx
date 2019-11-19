import React from "react";
import S from "./ScreenContainer.module.scss";

export const ScreenContainer: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);