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
import { MobXProviderContext, observer, Provider } from "mobx-react";
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

@observer
export class CWorkbenchPage extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return getWorkbench(this.context.application);
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
      <Provider workbench={this.workbench}>
        <WorkbenchPage
          sidebar={<CSidebar/>}
          mainbar={
            <MainBar>
              <CScreenToolbar/>
              <ScreenTabsArea>
                <CScreenTabbedViewHandleRow/>
                <Fullscreen isFullscreen={isFullscreen}>
                  <CScreenHeader/>
                  <CScreenContent/>
                </Fullscreen>
                <CDialogContent/>
                {/*<ScreenTabbedViewHandleRow>
                <ScreenTabbedViewHandle isActive={false} hasCloseBtn={true}>
                  Cubehór
                </ScreenTabbedViewHandle>
                <ScreenTabbedViewHandle isActive={true} hasCloseBtn={true}>
                  Mýtobjekte
                </ScreenTabbedViewHandle>
                <ScreenTabbedViewHandle isActive={false} hasCloseBtn={true}>
                  Mýtobjektgrúpn
                </ScreenTabbedViewHandle>
              </ScreenTabbedViewHandleRow>*/}
              </ScreenTabsArea>
            </MainBar>
          }
        />
      </Provider>
    );
  }
}
