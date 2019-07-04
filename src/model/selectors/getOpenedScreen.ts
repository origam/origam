import { IOpenedScreen, COpenedScreen } from "../types/IOpenedScreen";

export function getOpenedScreen(ctx: any): IOpenedScreen {
  let cn = ctx;
  while (true) {
    if (cn.$type === COpenedScreen) {
      return cn;
    }
    cn = cn.parent;
  }
}
