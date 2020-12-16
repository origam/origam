import { getGroupingConfiguration } from "../TablePanelView/getGroupingConfiguration";
import { getGrouper } from "./getGrouper";


export function getTotalGroupRowCount(ctx: any) {
  if (!getGroupingConfiguration(ctx).isGrouping) {
    return undefined;
  }
  return getGrouper(ctx).topLevelGroups
    .reduce((count, group) => count + group.rowCount, 0);
}
