import {getTablePanelView} from "./getTablePanelView";

export function getFixedColumnsCount(ctx: any) {
  return getTablePanelView(ctx).fixedColumnCount;
}