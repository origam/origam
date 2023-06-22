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

