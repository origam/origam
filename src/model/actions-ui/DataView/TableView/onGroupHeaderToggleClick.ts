import { flow } from "mobx";
import { getGrouper } from "model/selectors/DataView/getGrouper";
import { IGroupRow } from "gui/Components/ScreenElements/Table/TableRendering/types";

export function onGroupHeaderToggleClick(ctx: any) {
  return flow(function* onTableCellClick(
    event: any,
    groupHeader: IGroupRow
  ) {
      console.log("Bla");
      getGrouper(ctx).loadChildren(groupHeader.sourceGroup);
  });
}