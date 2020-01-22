import React from "react";
import S from "./SidebarRecordInfo.module.scss";

export const SidebarRecordInfo: React.FC<{ lines: string[] }> = props => (
  <div className={S.root}>
    {props.lines.map((line, idx) => (
      <p key={idx} dangerouslySetInnerHTML={{ __html: line }} />
    ))}
  </div>
);
