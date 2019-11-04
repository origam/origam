import { getSelectedRowId } from "../TablePanelView/getSelectedRowId";
import { getBindingParent } from "./getBindingParent";

export function getParentRowId(ctx: any) {
  const bp = getBindingParent(ctx);
  return bp ? getSelectedRowId(bp) : undefined;
}
