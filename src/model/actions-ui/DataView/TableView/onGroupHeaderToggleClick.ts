import { flow } from "mobx";
import { IRowGroup } from "model/entities/types/IRowGroup";
import { getGrouper } from "model/selectors/DataView/getGrouper";

export function onGroupHeaderToggleClick(ctx: any) {
  return flow(function* onTableCellClick(
    event: any,
    groupHeader: IRowGroup
  ) {
     getGrouper(ctx).loadChildren(groupHeader)
  });
}