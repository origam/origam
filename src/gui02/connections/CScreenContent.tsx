import React from "react";
import { observer } from "mobx-react";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { MobXProviderContext } from "mobx-react";
import { getOpenedScreenItems } from "model/selectors/getOpenedScreenItems";
import { CScreen } from "./CScreen";

@observer
export class CScreenContent extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  render() {
    const openedScreenItems = getOpenedScreenItems(this.workbench);
    return openedScreenItems.map(item => (
      <CScreen key={`${item.menuItemId}@${item.order}`} openedScreen={item} />
    ));
  }
}
