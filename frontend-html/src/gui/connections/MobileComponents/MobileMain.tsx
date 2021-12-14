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
import S from "./MobileMain.module.scss";
import { MobileToolBar } from "gui/connections/MobileComponents/MobileToolBar";
import { CScreenContent } from "gui/connections/CScreenContent";
import { CSidebar } from "gui/connections/CSidebar";
import { MobXProviderContext, observer } from "mobx-react";
import { MobileState } from "model/entities/MobileState";
import { MobileAboutView } from "gui/connections/MobileComponents/MobileAboutView";
import { About } from "model/entities/AboutInfo";
import { getAbout } from "model/selectors/getAbout";
import { MobileSearch } from "gui/connections/MobileComponents/MobileSearch";
import { MobileBottomBar } from "gui/connections/MobileComponents/MobileBottomBar";


@observer
export class MobileMain extends React.Component<{}> {

  static contextType = MobXProviderContext;

  get mobileState(): MobileState {
    return this.context.application.mobileState;
  }

  get about(): About {
    return getAbout(this.context.application);
  }

  componentDidMount() {
    this.about.update();
  }

  renderMainPageContents(){
    switch(this.mobileState.mainPageContents){
      case MainPageContents.Menu:
        return  <CSidebar/>;
      case MainPageContents.Screen:
        return <CScreenContent/>;
      case MainPageContents.Search:
        return <MobileSearch/>
      case MainPageContents.About:
        return <MobileAboutView
          aboutInfo={this.about.info}
          mobileState={this.mobileState}
        />
    }
  }

  render() {
    return (
        <div className={S.root}>
          <MobileToolBar mobileState={this.mobileState}/>
          {this.renderMainPageContents()}
          <MobileBottomBar/>
        </div>
    );
  }
}

export enum MainPageContents {
  Menu,
  Screen,
  About,
  Search,
}



