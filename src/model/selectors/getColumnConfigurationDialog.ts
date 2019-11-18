import { getTablePanelView } from "./TablePanelView/getTablePanelView";

export function getColumnConfigurationDialog(ctx: any) {
  return getTablePanelView(ctx).columnConfigurationDialog;
}
