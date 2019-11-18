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
import { onSaveSessionClick } from "model/actions/onSaveSessionClick";
import { onRefreshSessionClick } from "model/actions/onRefreshSessionClick";
import { observer } from "mobx-react";
import { getActiveScreenActions } from "model/selectors/getActiveScreenActions";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";
import { onActionClick } from "model/actions/Actions/onActionClick";

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

  render() {
    const activeScreen = getActiveScreen(this.application);
    const formScreen =
      activeScreen && !activeScreen.content.isLoading
        ? activeScreen.content.formScreen
        : undefined;
    const isDirty = formScreen && formScreen.isDirty;
    const toolbarActions = getActiveScreenActions(this.application);
    return (
      <ScreenToolbar>
        {formScreen ? (
          <>
            <ScreenToolbarActionGroup>
              <ScreenToolbarAction
                onClick={onSaveSessionClick(formScreen)}
                icon={<Icon src="./icons/save.svg" />}
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
                        onClick={event => onActionClick(action)(event, action)}
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
        <ScreenToolbarAction
          icon={<Icon src="./icons/search.svg" />}
          label="Search"
        />
        <ScreenToolbarAction
          onClick={this.handleLogoutClick}
          icon={
            <>
              <Icon src="./icons/user.svg" />
              {/*<ScreenToolbarAlertCounter>5</ScreenToolbarAlertCounter>*/}
            </>
          }
          label="User"
        />
      </ScreenToolbar>
    );
  }
}
