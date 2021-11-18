import React from "react";
import {Dropdown} from "gui/Components/Dropdown/Dropdown";
import {Dropdowner} from "gui/Components/Dropdowner/Dropdowner";
import {UserMenuBlock} from "gui/Components/UserMenuDropdown/UserMenuBlock";
import {DropdownItem} from "gui/Components/Dropdown/DropdownItem";
import {T} from "utils/translation";
import S from "gui/Components/UserMenuDropdown/UserMenuDropdown.module.scss";
import cx from "classnames";
import { getDialogStack } from "model/selectors/getDialogStack";
import { AboutDialog } from "gui/Components/Dialogs/AboutDialog";
import { IAboutInfo } from "model/entities/types/IAboutInfo";

export const UserMenuDropdown: React.FC<{
  handleLogoutClick: (event: any) => void,
  avatarLink: string | undefined,
  userName: string | undefined,
  ctx: any,
  aboutInfo: IAboutInfo,
  helpUrl: string | undefined
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
                <img className={cx(S.avatar, S.clickableAvatar)} src={props.avatarLink} />
              </div>
            </div>
            <div className={S.userNameLabel}>{props.userName}</div>
          </div>
        </div>
      )}
      content={({ setDropped }) => (
        <Dropdown>
          <UserMenuBlock
            userName={props.userName || "Logged user"}
            avatarLink={props.avatarLink}
            actionItems={
              <>
                {/* <DropdownItem isDisabled={true}>
                  {T("My profile", "my_profile")}
                </DropdownItem> */}
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
                    onAboutClick();
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