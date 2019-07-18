import {
  ILoadedFormScreen,
  isILoadedFormScreen
} from "../../entities/types/IFormScreen";

export function getFormScreen(ctx: any): ILoadedFormScreen {
  let cn = ctx;
  while (true) {
    if (isILoadedFormScreen(cn)) {
      return cn;
    }
    cn = cn.parent;
  }
}
