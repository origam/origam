import { getGroupingConfiguration } from "../TablePanelView/getGroupingConfiguration";
import { getDataView } from "./getDataView";
import { getGrouper } from "./getGrouper";


export function getTotalGroupRowCount(ctx: any) {
  if (!getGroupingConfiguration(ctx).isGrouping) {
    return getDataView(ctx).totalRowCount;
  }

  if(getGrouper(ctx).allGroups.some((group) => group.isExpanded)){
    const dataView = getDataView(ctx);
    return getGrouper(ctx).getTotalRowCount(dataView.selectedRowId!);
  }else{
      return getGrouper(ctx).topLevelGroups
        .reduce((count, group) => count + group.rowCount, 0);
  }
}
