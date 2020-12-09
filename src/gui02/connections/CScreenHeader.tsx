import { Icon } from "gui02/components/Icon/Icon";
import { ScreenHeader } from "gui02/components/ScreenHeader/ScreenHeader";
import { ScreenHeaderAction } from "gui02/components/ScreenHeader/ScreenHeaderAction";
import { ScreenHeaderPusher } from "gui02/components/ScreenHeader/ScreenHeaderPusher";
import { MobXProviderContext, observer } from "mobx-react";
import { onFullscreenClick } from "model/actions-ui/ScreenHeader/onFullscreenClick";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getIsScreenOrAnyDataViewWorking } from "model/selectors/FormScreen/getIsScreenOrAnyDataViewWorking";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { getIsCurrentScreenFull } from "model/selectors/Workbench/getIsCurrentScreenFull";
import React from "react";
import { getIsTopmostNonDialogScreen } from "model/selectors/getIsTopmostNonDialogScreen";
import { ScreenheaderDivider } from "gui02/components/ScreenHeader/ScreenHeaderDivider";
import { onWorkflowAbortClick } from "model/actions-ui/ScreenHeader/onWorkflowAbortClick";
import { onWorkflowNextClick } from "model/actions-ui/ScreenHeader/onWorkflowNextClick";

import S from "gui02/components/ScreenHeader/ScreenHeader.module.scss";
import { T } from "../../utils/translation";
import { ErrorBoundaryEncapsulated } from "gui02/components/Utilities/ErrorBoundary";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";

@observer
export class CScreenHeader extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  render() {
    const openedScreenItems = getOpenedNonDialogScreenItems(this.workbench);
    const activeScreen = openedScreenItems.find((item) => getIsTopmostNonDialogScreen(item));
    if (!activeScreen) {
      return null;
    }
    return (
      <ErrorBoundaryEncapsulated ctx={activeScreen}>
        <CScreenHeaderInner activeScreen={activeScreen} />
      </ErrorBoundaryEncapsulated>
    );
  }
}

@observer
class CScreenHeaderInner extends React.Component<{ activeScreen: IOpenedScreen }> {
  render() {
    const { activeScreen } = this.props;
    const { content } = activeScreen;
    const isFullscreen = getIsCurrentScreenFull(activeScreen);
    if (!content) return null;
    const isNextButton = content.formScreen && content.formScreen.showWorkflowNextButton;
    const isCancelButton = content.formScreen && content.formScreen.showWorkflowCancelButton;
    return (
      <ScreenHeader
        isLoading={content.isLoading || getIsScreenOrAnyDataViewWorking(content.formScreen!)}
      >
        <h1>{activeScreen.title}</h1>
        {(isCancelButton || isNextButton) && <ScreenheaderDivider />}
        {isCancelButton && (
          <button
            className={S.workflowActionBtn}
            onClick={onWorkflowAbortClick(content.formScreen!)}
          >
            {T("Cancel", "button_cancel")}
          </button>
        )}
        {isNextButton && (
          <button
            className={S.workflowActionBtn}
            onClick={onWorkflowNextClick(content.formScreen!)}
          >
            {T("Next", "button_next")}
          </button>
        )}
        <ScreenHeaderPusher />
        <ScreenHeaderAction onClick={onFullscreenClick(activeScreen)} isActive={isFullscreen}>
          <Icon
            src="./icons/fullscreen.svg"
            tooltip={T("Fullscreen", "fullscreen_button_tool_tip")}
          />
        </ScreenHeaderAction>
      </ScreenHeader>
    );
  }
}
