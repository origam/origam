import { TabbedViewHandle } from "gui02/components/TabbedView/TabbedViewHandle";
import { TabbedViewHandleRow } from "gui02/components/TabbedView/TabbedViewHandleRow";
import { MobXProviderContext, observer } from "mobx-react";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { onScreenTabHandleClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabHandleClick";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getIsFormScreenDirty } from "model/selectors/FormScreen/getisFormScreenDirty";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import React from "react";

@observer
export class CScreenTabbedViewHandleRow extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  getLabel(item: IOpenedScreen) {
    const text = item.title;
    const order = item.order > 0 ? `[${item.order}]` : "";
    return [text, order].join(" ");
  }

  render() {
    const openedScreenItems = getOpenedNonDialogScreenItems(this.workbench);

    return (
      <TabbedViewHandleRow>
        {openedScreenItems.map(item => (
          <TabbedViewHandle
            key={`${item.menuItemId}@${item.order}`}
            isActive={item.isActive}
            hasCloseBtn={true}
            isDirty={getIsFormScreenDirty(item)}
            onClick={(event: any) => onScreenTabHandleClick(item)(event)}
            onCloseClick={(event: any) => onScreenTabCloseClick(item)(event)}
          >
            {this.getLabel(item)}
          </TabbedViewHandle>
        ))}
      </TabbedViewHandleRow>
    );
  }
}
