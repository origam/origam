import {getTablePanelView} from "../TablePanelView/getTablePanelView";

export function getOrderingConfiguration(ctx: any) {
  return getTablePanelView(ctx).orderingConfiguration;
}