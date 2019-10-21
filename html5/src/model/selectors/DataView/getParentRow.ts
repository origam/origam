import { getBindingParents } from "./getBindingParents";
import { getSelectedRow } from "./getSelectedRow";

export function getParentRow(ctx: any) {
  const bps = getBindingParents(ctx);
  const bp = bps.length > 0 ? bps[0] : undefined;
  return bp ? getSelectedRow(bp) : undefined;
}