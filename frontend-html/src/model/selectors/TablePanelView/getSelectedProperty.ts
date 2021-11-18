import {getTablePanelView} from "./getTablePanelView";

export function getSelectedProperty(ctx: any) {
  return getTablePanelView(ctx).selectedProperty;
}
