import React from "react";
import S from "gui/Components/Form/Form.module.scss";

export const Form: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);