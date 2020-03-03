import { DialogScreen } from "gui/Workbench/ScreenArea/ScreenArea";
import { MobXProviderContext, observer } from "mobx-react";
import { getOpenedDialogScreenItems } from "model/selectors/getOpenedDialogScreenItems";
import React from "react";

@observer
export class CDialogContent extends React.Component {
  static contextType = MobXProviderContext;

  get workbench() {
    return this.context.workbench;
  }

  render() {
    const openedDialogItems = getOpenedDialogScreenItems(this.workbench);
    return (
      <>
        {openedDialogItems.map(item => (
          <DialogScreen openedScreen={item} key={`${item.menuItemId}@${item.order}`} />
        ))}
      </>
    );
  }
}
