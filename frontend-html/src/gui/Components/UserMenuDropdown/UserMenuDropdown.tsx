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
import { Dropdown } from "gui/Components/Dropdown/Dropdown";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { UserMenuBlock } from "gui/Components/UserMenuDropdown/UserMenuBlock";
import { DropdownItem } from "gui/Components/Dropdown/DropdownItem";
import { T } from "utils/translation";
import S from "gui/Components/UserMenuDropdown/UserMenuDropdown.module.scss";
import cx from "classnames";

export const UserMenuDropdown: React.FC<{
  handleLogoutClick: (event: any) => void,
  avatarLink: string | undefined,
  userName?: string,
  hideLabel?: boolean,
  ctx: any,
  onAboutClick: ()=> void;
  helpUrl: string | undefined
}> = (props) => {

  function onHelpClick() {
    window.open(props.helpUrl);
  }

  return (
    <Dropdowner
      style={{width: "auto"}} // TODO: Move to stylesheet
      trigger={({refTrigger, setDropped}) => (
        <div
          ref={refTrigger}
          onMouseDown={() => setDropped(true)}>
          <div className={S.avatarSection}>
            <div className={S.pictureSection}>
              <div className={S.avatarContainer}>
                <img className={cx(S.avatar, S.clickableAvatar)} src={props.avatarLink} alt=""/>
              </div>
            </div>
            {!props.hideLabel && <div className={S.userNameLabel}>{props.userName}</div>}
          </div>
        </div>
      )}
      content={({setDropped}) => (
        <Dropdown>
          <UserMenuBlock
            userName={props.userName || "Logged user"}
            avatarLink={props.avatarLink}
            actionItems={
              <>
                {props.helpUrl && props.helpUrl.trim() !== "" &&
                <DropdownItem
                  onClick={() => {
                    setDropped(false);
                    onHelpClick();
                  }}>
                  {T("Help", "help_button")}
                </DropdownItem>}
                <DropdownItem
                  onClick={() => {
                    setDropped(false);
                    props.onAboutClick();
                  }}>
                  {T("About", "about_application")}
                </DropdownItem>
                <DropdownItem onClick={props.handleLogoutClick} className="redItem">
                  {T("Log out", "sign_out_tool_tip")}
                </DropdownItem>
              </>
            }
          />
        </Dropdown>
      )}
    />);
}