import { getTablePanelView } from "./getTablePanelView";

export function getGroupingConfiguration(ctx: any) {
  return getTablePanelView(ctx).groupingConfiguration;
}