import { isIAction } from "model/entities/types/IAction";

export function getAction(ctx: any) {
  let cn = ctx;
  while (!isIAction(cn)) cn = cn.parent;
  return cn;
}