import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";

export function getFilterGroupManager(ctx: any) {
  return getTablePanelView(ctx).filterGroupManager;
}