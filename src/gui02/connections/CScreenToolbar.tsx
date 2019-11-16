import { Icon } from "gui02/components/Icon/Icon";
import { ScreenToolbar } from "gui02/components/ScreenToolbar/ScreenToolbar";
import { ScreenToolbarAction } from "gui02/components/ScreenToolbar/ScreenToolbarAction";
import { ScreenToolbarPusher } from "gui02/components/ScreenToolbar/ScreenToolbarPusher";
import { MobXProviderContext } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import React from "react";
import { action } from "mobx";
import { onScreenToolbarLogoutClick } from "model/actions-ui/ScreenToolbar/onScreenToolbarLogoutClick";

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
    return (
      <ScreenToolbar>
        {/*<ScreenToolbarActionGroup>
          <ScreenToolbarAction
            icon={<Icon src="./icons/change-status.svg" />}
            label="Action 1"
          />
          <ScreenToolbarAction
            icon={<Icon src="./icons/image.svg" />}
            label="Action 2"
          />
        </ScreenToolbarActionGroup>
        <ScreenToolbarActionGroup>
          <ScreenToolbarAction
            icon={<Icon src="./icons/invoice.svg" />}
            label="Action 3"
          />
          <ScreenToolbarAction
            icon={<Icon src="./icons/word.svg" />}
            label="Action 4"
          />
        </ScreenToolbarActionGroup>*/}
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
