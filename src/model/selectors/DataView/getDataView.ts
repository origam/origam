import { IDataView, isIDataView } from "../../entities/types/IDataView";

export function getDataView(ctx: any): IDataView {
  let cn = ctx;
  while (true) {
    if (isIDataView(cn)) {
      return cn;
    }
    if (!cn) return undefined as any;
    cn = cn?.parent;
  }
}
