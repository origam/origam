import { getGroupingConfiguration } from "../TablePanelView/getGroupingConfiguration";
import { getDataView } from "./getDataView";
import { getGrouper } from "./getGrouper";


export function getExpandedGroupRowCount(ctx: any) {
  if (!getGroupingConfiguration(ctx).isGrouping) {
    return undefined;
  }

  if (getGrouper(ctx).allGroups.some((group) => group.isExpanded)) {
    const dataView = getDataView(ctx);
    return getGrouper(ctx).getTotalRowCount(dataView.selectedRowId!);
  }

  else {
    return undefined;
  }
}
