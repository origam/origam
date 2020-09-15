import React from "react";
import S from "./UserMenuDropdown.module.scss";

export const UserMenuBlock: React.FC<{
  userName: string;
  avatarLink: string;
  actionItems: React.ReactNode;
}> = props => (
  <div className={S.root}>
    <div className="avatarSection">
      <div className="pictureSection">
        <div className="avatarContainer">
          <img className="avatar" src={props.avatarLink} />
        </div>
      </div>
      <div className="infoSection">{props.userName}</div>
    </div>
    <div className="actionSection">
      <div className="menuItemDivider" />
      {props.actionItems}
    </div>
  </div>
);
