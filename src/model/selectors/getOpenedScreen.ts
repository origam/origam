import { IOpenedScreen, isIOpenedScreen } from "../entities/types/IOpenedScreen";

export function getOpenedScreen(ctx: any): IOpenedScreen {
  let cn = ctx;
  while (true) {
    if (isIOpenedScreen(cn)) return cn;
    if (!cn) return undefined as any;
    cn = cn.parent;
  }
}
