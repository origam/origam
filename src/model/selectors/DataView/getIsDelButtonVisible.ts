import { getRowStateById } from "../RowState/getRowStateById";
import {getDataView} from "./getDataView";

export function getIsDelButtonVisible(ctx: any) {
  const dataView = getDataView(ctx);
  if(dataView.selectedRowId){
    const rowState = getRowStateById(ctx, dataView.selectedRowId);
    if(rowState?.allowDelete !== undefined){
      return dataView.showDeleteButton && rowState.allowDelete;
    }
  }
  return dataView.showDeleteButton
}
