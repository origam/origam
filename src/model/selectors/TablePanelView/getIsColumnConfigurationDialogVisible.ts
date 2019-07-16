import { getTablePanelView } from "./getTablePanelView";

export function getIsColumnConfigurationDialogVisible(ctx: any) {
  return getTablePanelView(ctx).isColumnConfigurationDialogVisible;
}