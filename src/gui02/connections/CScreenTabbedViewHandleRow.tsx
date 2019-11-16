import React from "react";
import { observer } from "mobx-react";
import { MobXProviderContext } from "mobx-react";
import { ScreenTabbedViewHandle } from "gui02/components/ScreenTabbedView/ScreenTabbedViewHandle";
import { ScreenTabbedViewHandleRow } from "gui02/components/ScreenTabbedView/ScreenTabbedViewHandleRow";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getOpenedScreenItems } from "model/selectors/getOpenedScreenItems";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import { onScreenTabHandleClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabHandleClick";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";

@observer
export class CScreenTabbedViewHandleRow extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  getLabel(item: IOpenedScreen) {
    const text = !item.content.isLoading
      ? item.content.formScreen!.title
      : item.title;
    const order = item.order > 0 ? `[${item.order}]` : "";
    return [text, order].join(" ");
  }

  render() {
    const openedScreenItems = getOpenedScreenItems(this.workbench);

    return (
      <ScreenTabbedViewHandleRow>
        {openedScreenItems.map(item => (
          <ScreenTabbedViewHandle
            key={`${item.menuItemId}@${item.order}`}
            isActive={item.isActive}
            hasCloseBtn={true}
            onClick={(event: any) => onScreenTabHandleClick(item)(event)}
            onCloseClick={(event: any) => onScreenTabCloseClick(item)(event)}
          >
            {this.getLabel(item)}
          </ScreenTabbedViewHandle>
        ))}
      </ScreenTabbedViewHandleRow>
    );
  }
}
