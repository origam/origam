import React from "react";
import S from "./SidebarRecordAudit.module.scss";

export const SidebarRecordAudit: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);