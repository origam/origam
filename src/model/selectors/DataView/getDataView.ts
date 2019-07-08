import { CDataView, IDataView } from "../../types/IDataView";

export function getDataView(ctx: any): IDataView {
  let cn = ctx;
  while (true) {
    if (cn.$type === CDataView) {
      return cn;
    }
    cn = cn.parent;
  }
}
