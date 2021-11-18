/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import React from "react";
import S from "gui/Components/UserMenuDropdown/UserMenuDropdown.module.scss";

export const UserMenuBlock: React.FC<{
  userName: string;
  avatarLink: string | undefined;
  actionItems: React.ReactNode;
}> = props => (
  <div className={S.root}>
    <div className={S.avatarSection}>
      <div className={S.pictureSection}>
        <div className={S.avatarContainer}>
          <img className={S.avatar} src={props.avatarLink} alt=""/>
        </div>
      </div>
      <div className={S.infoSection}>{props.userName}</div>
    </div>
    <div className={S.actionSection}>
      <div className={S.menuItemDivider}/>
      {props.actionItems}
    </div>
  </div>
);
