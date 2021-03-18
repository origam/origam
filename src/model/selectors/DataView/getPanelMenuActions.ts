import { getDataView } from "./getDataView";

export function getPanelMenuActions(ctx: any) {
  return getDataView(ctx).panelMenuActions;
}