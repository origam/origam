import { getDataView } from "./getDataView";
import { getBindingParents } from "./getBindingParents";
import { getSelectedRowId } from "../TablePanelView/getSelectedRowId";

export function getParentRowId(ctx: any) {
  const bps = getBindingParents(ctx);
  const bp = bps.length > 0 ? bps[0] : undefined;
  return getSelectedRowId(bp);
}