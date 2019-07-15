import { IOpenedScreen, isIOpenedScreen } from "../types/IOpenedScreen";

export function getOpenedScreen(ctx: any): IOpenedScreen {
  let cn = ctx;
  while (!isIOpenedScreen(cn)) cn = cn.parent;
  return cn;
}
