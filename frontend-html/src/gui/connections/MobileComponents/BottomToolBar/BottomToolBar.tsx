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

import React from "react";
import S from "gui/connections/MobileComponents/BottomToolBar/BottomToolBar.module.scss";
import "gui/connections/MobileComponents/BottomToolBar/BottomToolBar.module.scss";
import { BottomIcon } from "gui/connections/MobileComponents/BottomToolBar/BottomIcon";
import { MobileState } from "model/entities/MobileState/MobileState";
import { ActionDropUp } from "gui/connections/MobileComponents/BottomToolBar/ActionDropUp";
import { MobXProviderContext, observer } from "mobx-react";
import { geScreenActionButtonsState } from "model/actions-ui/ScreenToolbar/saveBottonVisible";
import { onSaveSessionClick } from "model/actions-ui/ScreenToolbar/onSaveSessionClick";
import { onRefreshSessionClick } from "model/actions-ui/ScreenToolbar/onRefreshSessionClick";
import { computed } from "mobx";
import { onWorkflowNextClick } from "model/actions-ui/ScreenHeader/onWorkflowNextClick";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import { Button } from "@origam/components";
import { T } from "utils/translation";
import { getActions, sortToGroups } from "model/selectors/DataView/getMobileActions";

@observer
export class BottomToolBar extends React.Component<{
  mobileState: MobileState,
  ctx: any
}> {

  static contextType = MobXProviderContext;

  get mobileState(): MobileState {
    return this.context.application.mobileState;
  }

  @computed
  get activeScreen() {
    return getActiveScreen(this.props.ctx)?.content?.formScreen;
  }


  showNextButton() {
    return this.activeScreen && this.activeScreen.showWorkflowNextButton;
  }

  render() {
    const actionButtonsState = geScreenActionButtonsState(this.props.ctx);

    const actions = getActions(this.props.ctx, this.mobileState);

    const buttons = [];
    if (this.props.mobileState.layoutState.showCloseButton(!!this.activeScreen)) {
      buttons.push(
        <BottomIcon
          key={"close"}
          iconPath={"./icons/noun-close-25798.svg"}
          onClick={async () => {
            await this.props.mobileState.close()
          }}
        />
      );
    }
    if (this.props.mobileState.layoutState.showOkButton) {
      buttons.push(
        <Button
          key={"ok"}
          label={T("Ok", "button_ok")}
          onClick={async () => {
            await this.props.mobileState.close()
          }}
        />
      );
    }
    if (actions.length > 0 && !this.props.mobileState.layoutState.actionDropUpHidden) {
      buttons.push(
        <ActionDropUp
          key={"actions"}
          actionGroups={sortToGroups(actions)}
        />
      );
    }
    if (!this.props.mobileState.layoutState.refreshButtonHidden && actionButtonsState?.isRefreshButtonVisible) {
      buttons.push(
        <BottomIcon
          key={"refresh"}
          iconPath={"./icons/noun-loading-1780489.svg"}
          onClick={onRefreshSessionClick(actionButtonsState?.formScreen)}
        />
      );
    }
    if (!this.props.mobileState.layoutState.saveButtonHidden && actionButtonsState?.isSaveButtonVisible) {
      buttons.push(
        <BottomIcon
          key={"save"}
          className={actionButtonsState?.isDirty ? S.isRed : ""}
          iconPath={"./icons/noun-save-1014816.svg"}
          onClick={onSaveSessionClick(actionButtonsState?.formScreen)}
        />
      );
    }
    if (this.showNextButton()) {
      buttons.push(
        <BottomIcon
          key={"next"}
          iconPath={"./icons/list-arrow-next.svg"}
          onClick={() => onWorkflowNextClick(this.activeScreen!)(null)}
        />
      );
    }
    return buttons.length !== 0
      ? <div className={S.root}>{buttons}</div>
      : null;
  }
}

