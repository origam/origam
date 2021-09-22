import {getRelationState as getRelationRowState} from "../RowState/getRelationState";
import {getDataView} from "./getDataView";
import {IPanelViewType} from "model/entities/types/IPanelViewType";
import {getActivePerspective} from "model/selectors/DataView/getActivePerspective";

export function getIsAddButtonVisible(ctx: any) {
  const dataView = getDataView(ctx);
  if(getActivePerspective(ctx) === IPanelViewType.Map){
    return false;
  }
  const relationRowState = getRelationRowState(ctx);
  if(relationRowState?.allowCreate !== undefined){
    return dataView.showAddButton && relationRowState.allowCreate;
  }
  return dataView.showAddButton
}