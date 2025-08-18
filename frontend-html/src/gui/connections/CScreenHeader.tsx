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

import { Icon } from "gui/Components/Icon/Icon";
import { ScreenHeader } from "gui/Components/ScreenHeader/ScreenHeader";
import { ScreenHeaderAction } from "gui/Components/ScreenHeader/ScreenHeaderAction";
import { ScreenHeaderPusher } from "gui/Components/ScreenHeader/ScreenHeaderPusher";
import { MobXProviderContext, observer } from "mobx-react";
import { onFullscreenClick } from "model/actions-ui/ScreenHeader/onFullscreenClick";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getIsScreenOrAnyDataViewWorking } from "model/selectors/FormScreen/getIsScreenOrAnyDataViewWorking";
import { getIsCurrentScreenFull } from "model/selectors/Workbench/getIsCurrentScreenFull";
import React from "react";
import { ScreenheaderDivider } from "gui/Components/ScreenHeader/ScreenHeaderDivider";
import { onWorkflowAbortClick } from "model/actions-ui/ScreenHeader/onWorkflowAbortClick";
import { onWorkflowNextClick } from "model/actions-ui/ScreenHeader/onWorkflowNextClick";

import S from "gui/Components/ScreenHeader/ScreenHeader.module.scss";
import { T } from "utils/translation";
import { ErrorBoundaryEncapsulated } from "gui/Components/Utilities/ErrorBoundary";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import { DataViewHeaderAction } from "gui/Components/DataViewHeader/DataViewHeaderAction";
import { isAddRecordShortcut, isSaveShortcut } from "utils/keyShortcuts";
import { ScreenToolbarAction } from "gui/Components/ScreenToolbar/ScreenToolbarAction";
import { WorkflowAction } from "gui/connections/WorkflowAction";
import { getIsTopmostNonDialogScreen } from "model/selectors/getIsTopmostNonDialogScreen";
import { getTopmostOpenedNonDialogScreenItem } from "model/selectors/getTopmostNonDialogScreenItem";

@observer
export class CScreenHeader extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  render() {
    const activeScreen = getActiveScreen(this.workbench);
    if (!activeScreen) {
      return null;
    }
    return (
      <ErrorBoundaryEncapsulated ctx={activeScreen}>
        <CScreenHeaderInner activeScreen={activeScreen}/>
      </ErrorBoundaryEncapsulated>
    );
  }
}

@observer
class CScreenHeaderInner extends React.Component<{ activeScreen: IOpenedScreen }> {
  render() {
    const {activeScreen} = this.props;
    const {content} = activeScreen;
    const isFullscreen = getIsCurrentScreenFull(activeScreen);
    if (!content) return null;
    const isTopMostNonDialogScreen = getIsTopmostNonDialogScreen(activeScreen);
    const formTitle = (activeScreen.formTitle || getTopmostOpenedNonDialogScreenItem(activeScreen)?.formTitle) ?? "";

    const isNextButton = content.formScreen && content.formScreen.showWorkflowNextButton && isTopMostNonDialogScreen;
    const isCancelButton = content.formScreen && content.formScreen.showWorkflowCancelButton && isTopMostNonDialogScreen;
    return (
      <>
        <h1 className={"printOnly"}>{formTitle}</h1>
        <ScreenHeader
          isLoading={content.isLoading || getIsScreenOrAnyDataViewWorking(content.formScreen!)}
        >
          <h1>{formTitle}</h1>
          {(isCancelButton || isNextButton) && <ScreenheaderDivider/>}
          {isCancelButton && (
            <WorkflowAction
              className={S.workflowActionBtn}
              onClick={onWorkflowAbortClick(content.formScreen!)}
              label= {T("Cancel", "button_cancel")}
            />
          )}
          {isNextButton && (
            <WorkflowAction
              className={S.workflowActionBtn}
              onClick={onWorkflowNextClick(content.formScreen!)}
              onShortcut={onWorkflowNextClick(content.formScreen!)}
              shortcutPredicate={isSaveShortcut}
              label= {T("Next", "button_next")}
            />
          )}
          <ScreenHeaderPusher/>
          <ScreenHeaderAction onClick={onFullscreenClick(activeScreen)} isActive={isFullscreen}>
            <Icon
              src="./icons/fullscreen.svg"
              tooltip={T("Fullscreen", "fullscreen_button_tool_tip")}
            />
          </ScreenHeaderAction>
        </ScreenHeader>
      </>
    );
  }
}
