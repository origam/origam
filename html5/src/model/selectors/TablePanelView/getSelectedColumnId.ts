import { getTablePanelView } from "./getTablePanelView";

export function getSelectedColumnId(ctx: any) {
  return getTablePanelView(ctx).selectedColumnId;
}
