import React from "react";
import { MobXProviderContext } from "mobx-react";
import { ScreenHeader } from "gui02/components/ScreenHeader/ScreenHeader";
import { ScreenheaderDivider } from "gui02/components/ScreenHeader/ScreenHeaderDivider";
import { ScreenHeaderAction } from "gui02/components/ScreenHeader/ScreenHeaderAction";
import { Icon } from "gui02/components/Icon/Icon";
import { ScreenHeaderPusher } from "gui02/components/ScreenHeader/ScreenHeaderPusher";
import { ScreenHeaderRowNo } from "gui02/components/ScreenHeader/ScreenHeaderRowNo";
import { observer } from "mobx-react";
import { getOpenedScreenItems } from "model/selectors/getOpenedScreenItems";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { IFormScreenEnvelope } from "model/entities/types/IFormScreen";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";

@observer
export class CScreenHeader extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  getLabel(openedScreen: IOpenedScreen) {
    return !openedScreen.content.isLoading
      ? openedScreen.content.formScreen!.title
      : openedScreen.title;
  }

  render() {
    const openedScreenItems = getOpenedScreenItems(this.workbench);
    const activeScreen = openedScreenItems.find(item => item.isActive);
    if (!activeScreen) {
      return null;
    }
    const { content } = activeScreen;
    if (content.isLoading) {
      return null;
    }

    return (
      <ScreenHeader>
        <h1>{this.getLabel(activeScreen)}</h1>
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
        <ScreenHeaderAction>
          <Icon src="./icons/fullscreen.svg" />
        </ScreenHeaderAction>
        {/*<ScreenHeaderAction>
          <Icon src="./icons/dot-menu.svg" />
        </ScreenHeaderAction>*/}
      </ScreenHeader>
    );
  }
}
