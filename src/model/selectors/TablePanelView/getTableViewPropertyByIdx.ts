import {getTablePanelView} from "./getTablePanelView";

export function getTableViewPropertyByIdx(ctx: any, idx: number) {
  return getTablePanelView(ctx).tableProperties[idx];
}
