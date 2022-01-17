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
import { getActiveScreenActions } from "model/selectors/getActiveScreenActions";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";
import { getPanelViewActions } from "model/selectors/DataView/getPanelViewActions";
import { IActionMode } from "model/entities/types/IAction";
import { computed } from "mobx";
import { onWorkflowNextClick } from "model/actions-ui/ScreenHeader/onWorkflowNextClick";
import { getActiveScreen } from "model/selectors/getActiveScreen";

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
  get activeScreen(){
    return getActiveScreen(this.props.ctx)?.content?.formScreen;
  }

  getActions(){
    const screenActions = getActiveScreenActions(this.props.ctx)
      .flatMap(actionGroup => actionGroup.actions)
      .filter(action => getIsEnabledAction(action));


    const dataViews = this.activeScreen?.dataViews;
    if(!dataViews || dataViews.length === 0){
      return screenActions;
    }

    let dataView;
    if( dataViews.length === 1){
      dataView = dataViews[0];
    }
    if(!this.mobileState.activeDataViewId){
      return screenActions;
    }
    dataView = dataViews.find(dataView => dataView.id === this.mobileState.activeDataViewId);
    if(!dataView){
      return screenActions;
    }
    const sectionActions = getPanelViewActions(dataView)
      .filter((action) => !action.groupId)
      .filter((action) => getIsEnabledAction(action) || action.mode !== IActionMode.ActiveRecord);

    return screenActions.concat(sectionActions);
  }

  showNextButton(){
    return this.activeScreen && this.activeScreen.showWorkflowNextButton;
  }

  render() {
    const actionButtonsState = geScreenActionButtonsState(this.props.ctx);

    const actions = this.getActions();

    return (
      <div className={S.root}>
        {this.props.mobileState.layoutState.showCloseButton(!!this.activeScreen) &&
          <BottomIcon
          iconPath={"./icons/noun-close-25798.svg"}
          onClick={async () => {
            await this.props.mobileState.close()
          }}
        />}
        {actions.length > 0 && !this.props.mobileState.layoutState.actionDropUpHidden &&
          <ActionDropUp
            actions={actions}
          />
        }
        {!this.props.mobileState.layoutState.refreshButtonHidden && actionButtonsState?.isRefreshButtonVisible &&
          <BottomIcon
            iconPath={"./icons/noun-loading-1780489.svg"}
            onClick={onRefreshSessionClick(actionButtonsState?.formScreen)}
          />
        }
        {!this.props.mobileState.layoutState.saveButtonHidden && actionButtonsState?.isSaveButtonVisible  &&
          <BottomIcon
            className={actionButtonsState?.isDirty ? S.isRed : ""}
            iconPath={"./icons/noun-save-1014816.svg"}
            onClick={onSaveSessionClick(actionButtonsState?.formScreen)}
          />
        }
        {this.showNextButton() &&
          <BottomIcon
            iconPath={"./icons/list-arrow-next.svg"}
            onClick={() => onWorkflowNextClick(this.activeScreen!)(null)}
          />
        }
      </div>
    );
  }
}



