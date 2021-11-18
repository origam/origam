import {getDataView} from "./getDataView";

export function getPanelViewActions(ctx: any) {
  return getDataView(ctx).panelViewActions;
}
