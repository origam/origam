import {flow} from "mobx";
import {getGrouper} from "model/selectors/DataView/getGrouper";
import {IGroupRow} from "gui/Components/ScreenElements/Table/TableRendering/types";

export function onGroupHeaderToggleClick(ctx: any) {
  return flow(function* onGroupHeaderToggleClick(
    event: any,
    groupHeader: IGroupRow
  ) {
      getGrouper(ctx).loadChildren(groupHeader.sourceGroup);
  });
}