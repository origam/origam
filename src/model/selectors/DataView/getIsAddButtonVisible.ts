import { getRelationState as getRelationRowState } from "../RowState/getRelationState";
import {getDataView} from "./getDataView";

export function getIsAddButtonVisible(ctx: any) {
  const dataView = getDataView(ctx);
  const relationRowState = getRelationRowState(ctx);
  if(relationRowState?.allowCreate !== undefined){
    return dataView.showAddButton && relationRowState.allowCreate;
  }
  return dataView.showAddButton
}