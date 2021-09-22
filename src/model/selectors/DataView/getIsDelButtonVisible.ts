import { getRowStateById } from "../RowState/getRowStateById";
import {getDataView} from "./getDataView";
import {getActivePerspective} from "model/selectors/DataView/getActivePerspective";
import {IPanelViewType} from "model/entities/types/IPanelViewType";

export function getIsDelButtonVisible(ctx: any) {
  const dataView = getDataView(ctx);
  if(getActivePerspective(ctx) === IPanelViewType.Map){
    return false;
  }
  if(dataView.selectedRowId){
    const rowState = getRowStateById(ctx, dataView.selectedRowId);
    if(rowState?.allowDelete !== undefined){
      return dataView.showDeleteButton && rowState.allowDelete;
    }
  }
  return dataView.showDeleteButton
}
