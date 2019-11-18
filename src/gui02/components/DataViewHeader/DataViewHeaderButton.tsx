import React from "react";
import S from "./DataViewHeaderButton.module.scss";

export const DataViewHeaderButton: React.FC<{
  onClick?(event: any): void;
}> = props => (
  <button className={S.root} onClick={props.onClick}>
    {props.children}
  </button>
);
