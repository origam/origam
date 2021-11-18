import { IOpenedScreen, isIOpenedScreen } from "model/entities/types/IOpenedScreen";

export function tryGetOpenedScreen(ctx: any): IOpenedScreen | undefined {
  if (!ctx) {
    return undefined;
  }
  let cn = ctx;
  while (!isIOpenedScreen(cn)) {
    cn = cn.parent;
    if (!cn) {
      return undefined;
    }
  }
  return cn;
}