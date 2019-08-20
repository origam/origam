import { getTablePanelView } from "./getTablePanelView";

export function getSelectedRowIndex(ctx: any) {
  return getTablePanelView(ctx).selectedRowIndex;
}