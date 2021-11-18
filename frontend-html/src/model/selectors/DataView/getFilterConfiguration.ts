import {getTablePanelView} from "../TablePanelView/getTablePanelView";

export function getFilterConfiguration(ctx: any) {
  return getTablePanelView(ctx).filterConfiguration;
}
