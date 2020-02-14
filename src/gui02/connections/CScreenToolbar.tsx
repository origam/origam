import { Icon } from "gui02/components/Icon/Icon";
import { ScreenToolbar } from "gui02/components/ScreenToolbar/ScreenToolbar";
import { ScreenToolbarAction } from "gui02/components/ScreenToolbar/ScreenToolbarAction";
import { ScreenToolbarPusher } from "gui02/components/ScreenToolbar/ScreenToolbarPusher";
import { MobXProviderContext } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import React from "react";
import { action } from "mobx";
import { onScreenToolbarLogoutClick } from "model/actions-ui/ScreenToolbar/onScreenToolbarLogoutClick";
import { ScreenToolbarActionGroup } from "gui02/components/ScreenToolbar/ScreenToolbarActionGroup";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import { onSaveSessionClick } from "model/actions-ui/ScreenToolbar/onSaveSessionClick";
import { onRefreshSessionClick } from "model/actions-ui/ScreenToolbar/onRefreshSessionClick";
import { observer } from "mobx-react";
import { getActiveScreenActions } from "model/selectors/getActiveScreenActions";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";

import uiActions from "model/actions-ui-tree";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { UserMenuDropdown } from "gui02/components/UserMenuDropdown/UserMenuDropdown";
import { UserMenuBlock } from "gui02/components/UserMenuDropdown/UserMenuBlock";
import { getLoggedUserName } from "model/selectors/User/getLoggedUserName";
import { DropdownItem } from "gui02/components/Dropdown/DropdownItem";
import { onReloadWebScreenClick } from "model/actions-ui/ScreenToolbar/onReloadWebScreen";
import { isIFormScreen, isIFormScreenEnvelope } from "model/entities/types/IFormScreen";
import { isIWebScreen } from "model/entities/types/IWebScreen";

@observer
export class CScreenToolbar extends React.Component<{}> {
  static contextType = MobXProviderContext;

  get application(): IApplication {
    return this.context.application;
  }

  @action.bound
  handleLogoutClick(event: any) {
    onScreenToolbarLogoutClick(this.application)(event);
  }

  renderForFormScreen() {
    const activeScreen = getActiveScreen(this.application);
    if (activeScreen && !activeScreen.content) return null;
    const formScreen =
      activeScreen && !activeScreen.content.isLoading ? activeScreen.content.formScreen : undefined;
    const isDirty = formScreen && formScreen.isDirty;
    const toolbarActions = getActiveScreenActions(this.application);
    const userName = getLoggedUserName(this.application);
    return (
      <ScreenToolbar>
        {formScreen ? (
          <>
            <ScreenToolbarActionGroup>
              <ScreenToolbarAction
                onClick={onSaveSessionClick(formScreen)}
                icon={
                  <Icon src="./icons/save.svg" className={isDirty ? "isRed isHoverGreen" : ""} />
                }
              />
              <ScreenToolbarAction
                onClick={onRefreshSessionClick(formScreen)}
                icon={<Icon src="./icons/refresh.svg" />}
              />
            </ScreenToolbarActionGroup>
            {toolbarActions
              .filter(actionGroup => actionGroup.actions.length > 0)
              .map(actionGroup => (
                <ScreenToolbarActionGroup>
                  {/*actionGroup.section*/}
                  {actionGroup.actions
                    .filter(action => getIsEnabledAction(action))
                    .map(action => (
                      <ScreenToolbarAction
                        icon={<Icon src="./icons/settings.svg" />}
                        label={action.caption}
                        onClick={event => uiActions.actions.onActionClick(action)(event, action)}
                      />
                    ))}
                </ScreenToolbarActionGroup>
              ))}
            {/*<ScreenToolbarActionGroup>
              <ScreenToolbarAction
                icon={<Icon src="./icons/invoice.svg" />}
                label="Action 3"
              />
              <ScreenToolbarAction
                icon={<Icon src="./icons/word.svg" />}
                label="Action 4"
              />
            </ScreenToolbarActionGroup>*/}
          </>
        ) : null}
        <ScreenToolbarPusher />
        {/*<ScreenToolbarAction
          icon={<Icon src="./icons/search.svg" />}
          label="Search"
        />*/}
        <Dropdowner
          style={{ width: "auto" }} // TODO: Move to stylesheet
          trigger={({ refTrigger, setDropped }) => (
            <ScreenToolbarAction
              rootRef={refTrigger}
              onClick={() => setDropped(true)}
              //onClick={this.handleLogoutClick}
              icon={
                <>
                  <Icon src="./icons/user.svg" />
                  {/*<ScreenToolbarAlertCounter>5</ScreenToolbarAlertCounter>*/}
                </>
              }
              label={userName}
            />
          )}
          content={() => (
            <UserMenuDropdown>
              <UserMenuBlock
                userName={userName || "Logged user"}
                actionItems={
                  <>
                    <DropdownItem isDisabled={true}>My profile</DropdownItem>
                    <DropdownItem onClick={this.handleLogoutClick}>Log out</DropdownItem>
                  </>
                }
              />
            </UserMenuDropdown>
          )}
        />
      </ScreenToolbar>
    );
  }

  renderForWebScreen() {
    const activeScreen = getActiveScreen(this.application);
    const userName = getLoggedUserName(this.application);
    return (
      <ScreenToolbar>
        <>
          <ScreenToolbarActionGroup>
            {/*<ScreenToolbarAction
                onClick={onSaveSessionClick(formScreen)}
                icon={
                  <Icon
                    src="./icons/save.svg"
                    className={isDirty ? "isRed isHoverGreen" : ""}
                  />
                }
              />*/}
            <ScreenToolbarAction
              onClick={onReloadWebScreenClick(activeScreen)}
              icon={<Icon src="./icons/refresh.svg" />}
            />
          </ScreenToolbarActionGroup>

          {/*<ScreenToolbarActionGroup>
              <ScreenToolbarAction
                icon={<Icon src="./icons/invoice.svg" />}
                label="Action 3"
              />
              <ScreenToolbarAction
                icon={<Icon src="./icons/word.svg" />}
                label="Action 4"
              />
            </ScreenToolbarActionGroup>*/}
        </>

        <ScreenToolbarPusher />
        {/*<ScreenToolbarAction
          icon={<Icon src="./icons/search.svg" />}
          label="Search"
        />*/}
        <Dropdowner
          style={{ width: "auto" }} // TODO: Move to stylesheet
          trigger={({ refTrigger, setDropped }) => (
            <ScreenToolbarAction
              rootRef={refTrigger}
              onClick={() => setDropped(true)}
              //onClick={this.handleLogoutClick}
              icon={
                <>
                  <Icon src="./icons/user.svg" />
                  {/*<ScreenToolbarAlertCounter>5</ScreenToolbarAlertCounter>*/}
                </>
              }
              label={userName}
            />
          )}
          content={() => (
            <UserMenuDropdown>
              <UserMenuBlock
                userName={userName || "Logged user"}
                actionItems={
                  <>
                    <DropdownItem isDisabled={true}>My profile</DropdownItem>
                    <DropdownItem onClick={this.handleLogoutClick}>Log out</DropdownItem>
                  </>
                }
              />
            </UserMenuDropdown>
          )}
        />
      </ScreenToolbar>
    );
  }

  renderDefault() {
    const userName = getLoggedUserName(this.application);
    return (
      <ScreenToolbar>
        <ScreenToolbarPusher />
        {/*<ScreenToolbarAction
          icon={<Icon src="./icons/search.svg" />}
          label="Search"
        />*/}
        <Dropdowner
          style={{ width: "auto" }} // TODO: Move to stylesheet
          trigger={({ refTrigger, setDropped }) => (
            <ScreenToolbarAction
              rootRef={refTrigger}
              onClick={() => setDropped(true)}
              //onClick={this.handleLogoutClick}
              icon={
                <>
                  <Icon src="./icons/user.svg" />
                  {/*<ScreenToolbarAlertCounter>5</ScreenToolbarAlertCounter>*/}
                </>
              }
              label={userName}
            />
          )}
          content={() => (
            <UserMenuDropdown>
              <UserMenuBlock
                userName={userName || "Logged user"}
                actionItems={
                  <>
                    <DropdownItem isDisabled={true}>My profile</DropdownItem>
                    <DropdownItem onClick={this.handleLogoutClick}>Log out</DropdownItem>
                  </>
                }
              />
            </UserMenuDropdown>
          )}
        />
      </ScreenToolbar>
    );
  }

  render() {
    const activeScreen = getActiveScreen(this.application);
    if (!activeScreen) {
      return this.renderDefault();
    }
    if (activeScreen.content && isIFormScreenEnvelope(activeScreen.content)) {
      return this.renderForFormScreen();
    }
    if (isIWebScreen(activeScreen)) {
      return this.renderForWebScreen();
    }
    return null;
  }
}
