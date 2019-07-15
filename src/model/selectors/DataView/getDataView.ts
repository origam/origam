import { IDataView, isIDataView } from "../../types/IDataView";

export function getDataView(ctx: any): IDataView {
  let cn = ctx;
  while (true) {
    if (isIDataView(cn)) {
      return cn;
    }
    cn = cn.parent;
  }
}
