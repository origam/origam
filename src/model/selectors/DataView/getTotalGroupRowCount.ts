import { getGroupingConfiguration } from "../TablePanelView/getGroupingConfiguration";
import { getDataView } from "./getDataView";
import { getGrouper } from "./getGrouper";


export function getTotalGroupRowCount(ctx: any) {
  if (!getGroupingConfiguration(ctx).isGrouping) {
    return getDataView(ctx).totalRowCount;
  }
  return getGrouper(ctx).topLevelGroups
    .reduce((count, group) => count + group.rowCount, 0);
}
