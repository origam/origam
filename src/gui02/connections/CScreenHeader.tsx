import { Icon } from "gui02/components/Icon/Icon";
import { ScreenHeader } from "gui02/components/ScreenHeader/ScreenHeader";
import { ScreenHeaderAction } from "gui02/components/ScreenHeader/ScreenHeaderAction";
import { ScreenHeaderPusher } from "gui02/components/ScreenHeader/ScreenHeaderPusher";
import { MobXProviderContext, observer } from "mobx-react";
import { onFullscreenClick } from "model/actions-ui/ScreenHeader/onFullscreenClick";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getIsScreenOrAnyDataViewWorking } from "model/selectors/FormScreen/getIsScreenOrAnyDataViewWorking";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import { getIsCurrentScreenFull } from "model/selectors/Workbench/getIsCurrentScreenFull";
import React from "react";
import { getIsTopmostNonDialogScreen } from "model/selectors/getIsTopmostNonDialogScreen";
import { ScreenheaderDivider } from "gui02/components/ScreenHeader/ScreenHeaderDivider";
import { onWorkflowAbortClick } from "model/actions-ui/ScreenHeader/onWorkflowAbortClick";
import { onWorkflowNextClick } from "model/actions-ui/ScreenHeader/onWorkflowNextClick";

@observer
export class CScreenHeader extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  getLabel(openedScreen: IOpenedScreen) {
    return openedScreen.title;
  }

  render() {
    const openedScreenItems = getOpenedNonDialogScreenItems(this.workbench);
    const activeScreen = openedScreenItems.find((item) => getIsTopmostNonDialogScreen(item));
    if (!activeScreen) {
      return null;
    }
    const { content } = activeScreen;
    /*if (content.isLoading) {
      return null;
    }*/
    const isFullscreen = getIsCurrentScreenFull(activeScreen);
    if (!content) return null;
    const isNextButton = content.formScreen && content.formScreen.showWorkflowNextButton;
    const isCancelButton = content.formScreen && content.formScreen.showWorkflowCancelButton;
    return (
      <ScreenHeader
        isLoading={content.isLoading || getIsScreenOrAnyDataViewWorking(content.formScreen!)}
      >
        <h1>{this.getLabel(activeScreen)}</h1>
        {(isCancelButton || isNextButton) && <ScreenheaderDivider />}
        {isCancelButton && (
          <ScreenHeaderAction
            className="isOrangeOnHover"
            onClick={onWorkflowAbortClick(content.formScreen!)}
          >
            <Icon src="./icons/close.svg" />
          </ScreenHeaderAction>
        )}
        {isNextButton && (
          <ScreenHeaderAction
            className="isGreenOnHover"
            onClick={onWorkflowNextClick(content.formScreen!)}
          >
            <Icon src="./icons/list-arrow-next.svg" />
          </ScreenHeaderAction>
        )}
        {/*<ScreenheaderDivider />
          <ScreenHeaderAction className="isGreenOnHover">
          <Icon src="./icons/add.svg" />
        </ScreenHeaderAction>
        <ScreenHeaderAction className="isOrangeOnHover">
          <Icon src="./icons/duplicate.svg" />
        </ScreenHeaderAction>*/}

        <ScreenHeaderPusher />

        {/*<ScreenHeaderAction>
          <Icon src="./icons/list-arrow-first.svg" />
        </ScreenHeaderAction>
        <ScreenHeaderAction>
          <Icon src="./icons/list-arrow-previous.svg" />
        </ScreenHeaderAction>
        <ScreenHeaderAction>
          <Icon src="./icons/list-arrow-next.svg" />
        </ScreenHeaderAction>
        <ScreenHeaderAction>
          <Icon src="./icons/list-arrow-last.svg" />
        </ScreenHeaderAction>
        <ScreenHeaderRowNo>2 / 27</ScreenHeaderRowNo>
        <ScreenheaderDivider />
        <ScreenHeaderAction>
          <Icon src="./icons/table-view.svg" />
        </ScreenHeaderAction>
        <ScreenHeaderAction isActive={true}>
          <Icon src="./icons/detail-view.svg" />
        </ScreenHeaderAction>
        <ScreenHeaderAction>
          <Icon src="./icons/search-filter.svg" />
        </ScreenHeaderAction>
        <ScreenheaderDivider />*/}
        <ScreenHeaderAction onClick={onFullscreenClick(activeScreen)} isActive={isFullscreen}>
          <Icon src="./icons/fullscreen.svg" />
        </ScreenHeaderAction>
        {/*<ScreenHeaderAction>
          <Icon src="./icons/dot-menu.svg" />
        </ScreenHeaderAction>*/}
      </ScreenHeader>
    );
  }
}
