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
