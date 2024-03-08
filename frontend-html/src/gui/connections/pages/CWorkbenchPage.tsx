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

import { MainBar } from "gui/Components/MainBar/MainBar";
import { ScreenTabsArea } from "gui/Components/ScreenTabsArea/ScreenTabsArea";
import { WorkbenchPage } from "gui/Components/WorkbenchPage/WorkbenchPage";
import { MobXProviderContext, observer } from "mobx-react";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getWorkbench } from "model/selectors/getWorkbench";
import React from "react";
import { CScreenTabbedViewHandleRow } from "gui/connections/CScreenTabbedViewHandleRow";
import { CScreenToolbar } from "gui/connections/CScreenToolbar";
import { CSidebar } from "gui/connections/CSidebar";
import { CScreenHeader } from "gui/connections/CScreenHeader";
import { CScreenContent } from "gui/connections/CScreenContent";
import { CDialogContent } from "gui/connections/CDialogContent";
import { getIsCurrentScreenFull } from "model/selectors/Workbench/getIsCurrentScreenFull";
import { Fullscreen } from "gui/Components/Fullscreen/Fullscreen";
import { onRootElementClick } from "model/actions/Global/onRootElementClick";
import { action } from "mobx";
import S from "gui/Components/WorkbenchPage/WorkbenchPage.module.scss";
import { MobileMain } from "gui/connections/MobileComponents/MobileMain";
import { NotificationContainer } from "gui/connections/NotificationContainer";
import { IApplication } from "model/entities/types/IApplication";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";

@observer
export class CWorkbenchPage extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return getWorkbench(this.context.application);
  }

  get application() {
    return this.context.application as IApplication;
  }

  @action.bound
  handleRootElementClick(event: any) {
    return onRootElementClick(this.workbench)(event);
  }

  componentDidMount() {
    window.document.addEventListener("click", this.handleRootElementClick, true);
  }

  componentWillUnmount() {
    window.document.removeEventListener("click", this.handleRootElementClick, true);
  }

  render() {
    const isFullscreen = getIsCurrentScreenFull(this.workbench);
    return (
        <>
        {isMobileLayoutActive(this.application)
          ? <div className={S.mobileContainer}>
              <MobileMain/>
            </div>
          : <div className={S.root}>
              <WorkbenchPage
                sidebar={<CSidebar/>}
                mainbar={
                  <MainBar>
                    <CScreenToolbar/>
                    <ScreenTabsArea>
                      <CScreenTabbedViewHandleRow/>
                      <Fullscreen isFullscreen={isFullscreen}>
                        <CScreenHeader/>
                        <NotificationContainer/>
                        <CScreenContent/>
                      </Fullscreen>
                      <CDialogContent/>
                    </ScreenTabsArea>
                  </MainBar>
                }
              />
            </div>
        }
        </>
    );
  }
}


