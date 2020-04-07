import { flow } from "mobx";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { IRowGroup } from "model/entities/types/IRowGroup";

export function onGroupHeaderToggleClick(ctx: any) {
  return flow(function* onTableCellClick(
    event: any,
    groupHeader: IRowGroup
  ) {
    
  });
}