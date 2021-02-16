import React from "react";
import S from "gui/Components/DataViewHeader/DataViewHeaderPusher.module.scss";

export const DataViewHeaderPusher: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);