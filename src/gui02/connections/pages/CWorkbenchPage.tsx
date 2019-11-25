import { MainBar } from "gui02/components/MainBar/MainBar";
import { ScreenTabsArea } from "gui02/components/ScreenTabsArea/ScreenTabsArea";
import { WorkbenchPage } from "gui02/components/WorkbenchPage/WorkbenchPage";
import { MobXProviderContext, observer, Provider } from "mobx-react";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getWorkbench } from "model/selectors/getWorkbench";
import React from "react";
import { CScreenTabbedViewHandleRow } from "../CScreenTabbedViewHandleRow";
import { CScreenToolbar } from "../CScreenToolbar";
import { CSidebar } from "../CSidebar";
import { CScreenHeader } from "../CScreenHeader";
import { CScreenContent } from "../CScreenContent";
import { CDialogContent } from "../CDialogContent";
import { observable } from "mobx";
import { getIsCurrentScreenFull } from "model/selectors/Workbench/getIsCurrentScreenFull";
import { Fullscreen } from "gui02/components/Fullscreen/Fullscreen";

@observer
export class CWorkbenchPage extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return getWorkbench(this.context.application);
  }

  render() {
    const isFullscreen = getIsCurrentScreenFull(this.workbench);
    return (
      <Provider workbench={this.workbench}>
        <WorkbenchPage
          sidebar={<CSidebar />}
          mainbar={
            <MainBar>
              <CScreenToolbar />
              <ScreenTabsArea>
                <CScreenTabbedViewHandleRow />
                <Fullscreen isFullscreen={isFullscreen}>
                  <CScreenHeader />
                  <CScreenContent />
                </Fullscreen>
                <CDialogContent />
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
