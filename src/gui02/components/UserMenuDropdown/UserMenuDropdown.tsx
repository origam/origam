import React from "react";
import {Dropdown} from "../Dropdown/Dropdown";
import {Dropdowner} from "gui/Components/Dropdowner/Dropdowner";
import {Icon} from "gui02/components/Icon/Icon";
import {UserMenuBlock} from "gui02/components/UserMenuDropdown/UserMenuBlock";
import {DropdownItem} from "gui02/components/Dropdown/DropdownItem";
import {T} from "utils/translation";
import S from "./UserMenuDropdown.module.scss";

export const UserMenuDropdown: React.FC<{
  handleLogoutClick: (event: any) => void,
  avatarLink: string,
  userName: string | undefined
}> = (props) => {
  return (
    <Dropdowner
      style={{width: "auto"}} // TODO: Move to stylesheet
      trigger={({refTrigger, setDropped}) => (
        <div
          ref={refTrigger}
          onClick={() => setDropped(true)}>
          {
            props.avatarLink
              ? <div className={S.avatarSection}>
                <div className={S.pictureSection}>
                  <div className={S.avatarContainer}>
                    <img className={S.avatar} src={props.avatarLink} />
                  </div>
                </div>
                <div className={S.userNameSection}>{props.userName}</div>
              </div>
              : <Icon src="./icons/user.svg" tooltip={""}/>
          }
        </div>
      )}
      content={() => (
        <Dropdown>
          <UserMenuBlock
            userName={props.userName || "Logged user"}
            avatarLink={props.avatarLink}
            actionItems={
              <>
                <DropdownItem isDisabled={true}>
                  {T("My profile", "my_profile")}
                </DropdownItem>
                <DropdownItem onClick={props.handleLogoutClick}>
                  {T("Log out", "sign_out_tool_tip")}
                </DropdownItem>
              </>
            }
          />
        </Dropdown>
      )}
    />);
}