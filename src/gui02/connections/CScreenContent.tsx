import { ScreenContainer } from "gui02/components/Screen/ScreenContainer";
import { MobXProviderContext, observer } from "mobx-react";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { getOpenedNonDialogScreenItems } from "model/selectors/getOpenedNonDialogScreenItems";
import React from "react";
import { CScreen } from "./CScreen";

@observer
export class CScreenContent extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  render() {
    const openedScreenItems = getOpenedNonDialogScreenItems(this.workbench);
    return (
      <ScreenContainer>
        {openedScreenItems.map(item => (
          <CScreen key={`${item.menuItemId}@${item.order}`} openedScreen={item} />
        ))}
      </ScreenContainer>
    );
  }
}
