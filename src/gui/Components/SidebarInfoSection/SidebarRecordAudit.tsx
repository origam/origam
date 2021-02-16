import React from "react";
import S from "gui/Components/SidebarInfoSection/SidebarRecordAudit.module.scss";

export const SidebarRecordAudit: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);