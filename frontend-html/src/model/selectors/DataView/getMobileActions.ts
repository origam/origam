/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
import { MobileState } from "model/entities/MobileState/MobileState";
import { getActiveScreenActions } from "model/selectors/getActiveScreenActions";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import { getPanelViewActions } from "model/selectors/DataView/getPanelViewActions";
import { IActionMode } from "model/entities/types/IAction";
import { isIFormScreenEnvelope } from "model/entities/types/IFormScreen";


export function getAllActions(ctx: any, mobileState: MobileState) {

  const activeScreen = getActiveScreen(ctx);
  const webScreenIsActive = !activeScreen?.content || !isIFormScreenEnvelope(activeScreen.content);
  if (webScreenIsActive) {
    return [];
  }
  const screenActions = getActiveScreenActions(ctx)
    .flatMap(actionGroup => actionGroup.actions)
    .filter(action => getIsEnabledAction(action));

  const activeFormScreen = activeScreen?.content?.formScreen;

  const dataViews = activeFormScreen?.dataViews;
  if (!dataViews || dataViews.length === 0) {
    return screenActions;
  }

  let dataView;
  if (dataViews.length === 1) {
    dataView = dataViews[0];
  }
  if (!mobileState.activeDataViewId) {
    return screenActions;
  }
  dataView = dataViews.find(dataView => dataView.id === mobileState.activeDataViewId);
  if (!dataView) {
    return screenActions;
  }
  const sectionActions = getPanelViewActions(dataView)
    .filter((action) => !action.groupId)
    .filter((action) => getIsEnabledAction(action) || action.mode !== IActionMode.ActiveRecord);

  return screenActions.concat(sectionActions);
}

