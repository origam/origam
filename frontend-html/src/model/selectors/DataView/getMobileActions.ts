import { MobileState } from "model/entities/MobileState/MobileState";
import { getActiveScreenActions } from "model/selectors/getActiveScreenActions";
import { getIsEnabledAction } from "model/selectors/Actions/getIsEnabledAction";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import { getPanelViewActions } from "model/selectors/DataView/getPanelViewActions";
import { IAction, IActionMode } from "model/entities/types/IAction";


export function getActions(ctx: any, mobileState: MobileState) {
  const screenActions = getActiveScreenActions(ctx)
    .flatMap(actionGroup => actionGroup.actions)
    .filter(action => getIsEnabledAction(action));

  const activeScreen = getActiveScreen(ctx)?.content?.formScreen;

  const dataViews = activeScreen?.dataViews;
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

export function sortToGroups(actions: IAction[]): IAction[][] {
  const groupMap = actions.groupBy(action => action.groupId);
  if(groupMap.size === 0){
    return [];
  }
  const noGroupActions = groupMap.get("");
  if(!noGroupActions){
    return []
  }
  const groups = [];
  for (let action of noGroupActions) {
    if(groupMap.has(action.id)){
      groups.push(groupMap.get(action.id)!)
    }
    else
    {
      let lastGroup = groups[groups.length -1];
      if(lastGroup && (lastGroup.length === 0 || lastGroup[0].groupId === "")) {
        lastGroup.push(action);
      }
      else
      {
        groups.push([action])
      }
    }
  }
  return groups;
}


