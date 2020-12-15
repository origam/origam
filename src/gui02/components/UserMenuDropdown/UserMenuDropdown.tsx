import React from "react";
import {Dropdown} from "../Dropdown/Dropdown";
import {Dropdowner} from "gui/Components/Dropdowner/Dropdowner";
import {UserMenuBlock} from "gui02/components/UserMenuDropdown/UserMenuBlock";
import {DropdownItem} from "gui02/components/Dropdown/DropdownItem";
import {T} from "utils/translation";
import S from "./UserMenuDropdown.module.scss";
import cx from "classnames";
import { getDialogStack } from "model/selectors/getDialogStack";
import { AboutDialog } from "gui/Components/Dialogs/AboutDialog";
import { IAboutInfo } from "model/entities/types/IAboutInfo";

export const UserMenuDropdown: React.FC<{
  handleLogoutClick: (event: any) => void,
  avatarLink: string | undefined,
  userName: string | undefined,
  ctx: any,
  aboutInfo: IAboutInfo
}> = (props) => {

  function onAboutClick() {
    const closeDialog = getDialogStack(props.ctx).pushDialog(
      "",
      <AboutDialog
        aboutInfo={props.aboutInfo}
        onOkClick={() => {
          closeDialog();
        }}
      />
    );
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
                <img className={cx(S.avatar, S.clickableAvatar)} src={props.avatarLink} />
              </div>
            </div>
            <div className={S.userNameLabel}>{props.userName}</div>
          </div>
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
                <DropdownItem onClick={onAboutClick}>
                  {T("About", "about_application")}
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