import { isInfiniteScrollingActive } from "../isInfiniteScrollingActive";
import { getGroupingConfiguration } from "../TablePanelView/getGroupingConfiguration";
import { getDataView } from "./getDataView";
import { getGrouper } from "./getGrouper";


export function getTotalRowCount(ctx: any) {
  if (!getGroupingConfiguration(ctx).isGrouping) {
    if(isInfiniteScrollingActive(ctx)){
      return getDataView(ctx).totalRowCount;
    }else{
      return getDataView(ctx).dataTable.rows.length;
    }
  }
  else
  {
    return getGrouper(ctx).topLevelGroups
      .reduce((count, group) => count + group.rowCount, 0);
  }
}
