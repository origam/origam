import {getDataView} from "./getDataView";

export function getAreCrudButtonsEnabled(ctx: any) {
  const dataView = getDataView(ctx);
  if(dataView.parentBindings.length === 0){
    return true;
  }
  return dataView.parentBindings.some(binding => binding.parentDataView.selectedRowId)
}