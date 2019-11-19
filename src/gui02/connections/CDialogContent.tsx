import React from "react";
import { observer } from "mobx-react";
import { MobXProviderContext } from "mobx-react";
import { getOpenedDialogItems } from "model/selectors/getOpenedDialogItems";
import { DialogScreen } from "gui/Workbench/ScreenArea/ScreenArea";

@observer
export class CDialogContent extends React.Component {
  static contextType = MobXProviderContext;

  get workbench() {
    return this.context.workbench;
  }

  render() {
    const openedDialogItems = getOpenedDialogItems(this.workbench);
    return (
      <>
        {openedDialogItems.map(item => (
          <DialogScreen
            openedScreen={item}
            key={`${item.menuItemId}@${item.order}`}
          />
        ))}
      </>
    );
  }
}
