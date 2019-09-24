import { IFormScreen, isIFormScreen } from "model/entities/types/IFormScreen";

export function getFormScreen(ctx: any): IFormScreen {
  let cn = ctx;
  while (true) {
    if (isIFormScreen(cn)) {
      return cn;
    }
    cn = cn.parent;
  }
}
