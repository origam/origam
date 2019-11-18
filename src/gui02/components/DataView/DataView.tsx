import React from "react";
import S from "./DataView.module.scss";

export const DataView: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);
