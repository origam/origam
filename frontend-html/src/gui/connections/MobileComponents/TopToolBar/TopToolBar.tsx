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
import S from "gui/connections/MobileComponents/TopToolBar/TopToolBar.module.scss";
import { getHelpUrl } from "model/selectors/User/getHelpUrl";
import { UserMenuDropdown } from "gui/Components/UserMenuDropdown/UserMenuDropdown";
import { MobXProviderContext, observer } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import { getUserAvatarLink } from "model/selectors/User/getUserAvatarLink";
import { action, computed } from "mobx";
import { onScreenToolbarLogoutClick } from "model/actions-ui/ScreenToolbar/onScreenToolbarLogoutClick";
import { getLoggedUserName } from "model/selectors/User/getLoggedUserName";
import { TabSelector } from "gui/connections/MobileComponents/TopToolBar/TabSelector";
import { SearchButton } from "gui/connections/MobileComponents/TopToolBar/SearchButton";
import { MenuButton } from "gui/connections/MobileComponents/MenuButton";
import { MobileState } from "model/entities/MobileState/MobileState";
import { AboutLayoutState, TopLeftComponent } from "model/entities/MobileState/MobileLayoutState";
import { Icon } from "gui/Components/Icon/Icon";
import { getActiveScreen } from "model/selectors/getActiveScreen";

@observer
export class TopToolBar extends React.Component<{
  mobileState: MobileState
}> {
  static contextType = MobXProviderContext;

  get application(): IApplication {
    return this.context.application;
  }

  get layoutState(){
    return this.props.mobileState.layoutState;
  }

  @computed
  get activeScreen() {
    return getActiveScreen(this.application.workbench)?.content?.formScreen;
  }

  @action.bound
  handleLogoutClick(event: any) {
    onScreenToolbarLogoutClick(this.application)(event);
  }

  getLeftElement(){
    if(this.layoutState.topLeftComponent === TopLeftComponent.Menu) {
      return <MenuButton/>;
    }
    else if(this.layoutState.topLeftComponent === TopLeftComponent.Close) {
      if (this.layoutState.showCloseButton(!!this.activeScreen)) {
        return (
         <div className={S.toCloseButton}>< Icon 
              src={"./icons/close-mobile.svg"}
              onClick={async () => {
                await this.props.mobileState.close()
              }}
            />
          </div> );
      }
    }
    else if(this.layoutState.topLeftComponent === TopLeftComponent.None) {
      return null;
    }
    else {
      throw new Error("Unsupported top left component: " + this.layoutState.topLeftComponent);
    }
  }

  render() {
    const avatarLink = getUserAvatarLink(this.application);
    const userName = getLoggedUserName(this.application);
    return (
      <div className={S.root}>
        <div className={S.sideContainer}>
          {this.getLeftElement()}
        </div>
          <TabSelector mobileState={this.props.mobileState}/>
        <div className={S.sideContainer}>
          {this.layoutState.showSearchButton && <SearchButton mobileState={this.props.mobileState}/>}
          <UserMenuDropdown
            avatarLink={avatarLink}
            handleLogoutClick={(event) => this.handleLogoutClick(event)}
            userName={userName}
            hideLabel={true}
            ctx={this.application}
            onAboutClick={() => this.props.mobileState.layoutState = new AboutLayoutState()}
            helpUrl={getHelpUrl(this.application)}
            />
        </div>
      </div>
    );
  }
}



