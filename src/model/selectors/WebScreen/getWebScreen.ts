import { isIWebScreen } from "model/entities/types/IWebScreen";

export function getWebScreen(ctx: any) {
  let cn = ctx;
  while (true) {
    if (isIWebScreen(cn)) {
      return cn;
    }
    cn = cn.parent;
  }
}