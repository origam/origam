import { IPanelViewType } from "../../entities/types/IPanelViewType";
import { getDataView } from "./getDataView";

export function getActivePanelView(ctx: any): IPanelViewType {
  return getDataView(ctx).activePanelView;
}
