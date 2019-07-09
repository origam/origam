import { CFormScreen, IFormScreen, ILoadedFormScreen } from "../../types/IFormScreen";

export function getFormScreen(ctx: any): ILoadedFormScreen {
  let cn = ctx;
  while (true) {
    if (cn.$type === CFormScreen) {
      return cn;
    }
    cn = cn.parent;
  }
}
