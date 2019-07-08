import { IOpenedScreen, COpenedScreen } from "../types/IOpenedScreen";

export function getOpenedScreen(ctx: any): IOpenedScreen {
  let cn = ctx;
  while (true) {
    console.log(cn)
    if (cn.$type === COpenedScreen) {
      return cn;
    }
    cn = cn.parent;
  }
}
