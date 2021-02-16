import React from "react";
import S from "gui/Components/Form/FormSectionHeader.module.scss";

export const FormSectionHeader: React.FC<{
  foreGroundColor: string | undefined;
  tooltip?: string;
}> = (props) => (
  <h1 className={S.root} style={{ color: props.foreGroundColor }} title={props.tooltip}>
    {props.children}
  </h1>
);
