import React from "react";
import S from "gui/Components/Dropdown/Dropdown.module.scss";

export const Dropdown: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);