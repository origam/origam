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
import S from "./BottomToolBar.module.scss";
import { BottomIcon } from "gui/connections/MobileComponents/BottomIcon";
import { MobileState } from "model/entities/MobileState";
import { ActionDropUp } from "gui/connections/MobileComponents/ActionDropUp";
import { observer } from "mobx-react";
import { geScreenActionButtonsState } from "model/actions-ui/ScreenToolbar/saveBottonVisible";
import { onSaveSessionClick } from "model/actions-ui/ScreenToolbar/onSaveSessionClick";
import { onRefreshSessionClick } from "model/actions-ui/ScreenToolbar/onRefreshSessionClick";
import { getActiveScreenActions } from "model/selectors/getActiveScreenActions";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";

@observer
export class BottomToolBar extends React.Component<{
  mobileState: MobileState,
  ctx: any
}> {
  render() {

    const actionButtonsState = geScreenActionButtonsState(this.props.ctx);
    const actions = getActiveScreenActions(this.props.ctx)
      .flatMap(actionGroup => actionGroup.actions)
      .filter(action => getIsEnabledAction(action));

    return (
      <div className={S.root}>
        <BottomIcon
          iconPath={"./icons/noun-close-25798.svg"}
          onClick={async () => {
            await this.props.mobileState.close()
          }}
        />
        {actions.length > 0 && !this.props.mobileState.layoutState.actionDropUpHidden &&
          <ActionDropUp
            actions={actions}
          />
        }
        {!this.props.mobileState.layoutState.refreshButtonHidden && actionButtonsState?.isSaveButtonVisible &&
          <BottomIcon
            iconPath={"./icons/noun-loading-1780489.svg"}
            onClick={onRefreshSessionClick(actionButtonsState?.formScreen)}
          />
        }
        {!this.props.mobileState.layoutState.saveButtonHidden && actionButtonsState?.isRefreshButtonVisible &&
          <BottomIcon
            className={actionButtonsState?.isDirty ? S.isRed : ""}
            iconPath={"./icons/noun-save-1014816.svg"}
            onClick={onSaveSessionClick(actionButtonsState?.formScreen)}
          />
        }
      </div>
    );
  }
}



