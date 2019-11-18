import React from "react";
import S from "./DataViewHeader.module.scss";

export const DataViewHeader: React.FC<{ domRef?: any }> = props => (
  <div className={S.root}>
    <div ref={props.domRef} className={S.inner}>
      {props.children}
    </div>
  </div>
);
