import React from "react";
import S from "./UserMenuDropdown.module.scss";

export const UserMenuBlock: React.FC<{
  userName: string;
  avatarLink: string;
  actionItems: React.ReactNode;
}> = props => (
  <div className={S.root}>
    <div className={S.avatarSection}>
      <div className={S.pictureSection}>
        <div className={S.avatarContainer}>
          <img className={S.avatar} src={props.avatarLink} />
        </div>
      </div>
      <div className={S.infoSection}>{props.userName}</div>
    </div>
    <div className={S.actionSection}>
      <div className={S.menuItemDivider} />
      {props.actionItems}
    </div>
  </div>
);
