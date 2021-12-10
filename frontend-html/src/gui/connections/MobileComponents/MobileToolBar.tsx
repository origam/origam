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
import S from "./MobileToolBar.module.scss";
import { MobileState } from "model/entities/MobileState";
import { MobileTabs } from "gui/connections/MobileComponents/MobileTabs";
import { Icon } from "@origam/components";
import { getHelpUrl } from "model/selectors/User/getHelpUrl";
import { UserMenuDropdown } from "gui/Components/UserMenuDropdown/UserMenuDropdown";
import { MobXProviderContext } from "mobx-react";
import { IApplication } from "model/entities/types/IApplication";
import { getUserAvatarLink } from "model/selectors/User/getUserAvatarLink";
import { action, observable } from "mobx";
import { onScreenToolbarLogoutClick } from "model/actions-ui/ScreenToolbar/onScreenToolbarLogoutClick";
import { getLoggedUserName } from "model/selectors/User/getLoggedUserName";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { getApi } from "model/selectors/getApi";
import { IAboutInfo } from "model/entities/types/IAboutInfo";
import { About } from "model/entities/AboutInfo";
import { getAbout } from "model/selectors/getAbout";

export class MobileToolBar extends React.Component<{
  mobileState: MobileState
}> {
  static contextType = MobXProviderContext;

  get application(): IApplication {
    return this.context.application;
  }

  @action.bound
  handleLogoutClick(event: any) {
    onScreenToolbarLogoutClick(this.application)(event);
  }

  get about(): About {
    return getAbout(this.application);
  }

  componentDidMount() {
    this.about.update();
  }

  render() {
    const avatarLink = getUserAvatarLink(this.application);
    const userName = getLoggedUserName(this.application);
    return (
      <div className={S.root}>
        <div
          onClick={() => this.props.mobileState.showMenu = !this.props.mobileState.showMenu}
        >
          <Icon
            src={"./icons/noun-hamburger.svg"}
            className={S.menuIcon}
          />
        </div>
        <MobileTabs mobileState={this.props.mobileState}/>
        <UserMenuDropdown
          avatarLink={avatarLink}
          handleLogoutClick={(event) => this.handleLogoutClick(event)}
          userName={userName}
          hideLabel={true}
          ctx={this.application}
          aboutInfo={this.about.info}
          helpUrl={getHelpUrl(this.application)}
        />
      </div>
    );
  }
}



