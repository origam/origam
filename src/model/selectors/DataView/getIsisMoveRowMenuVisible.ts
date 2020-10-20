import { getDataView } from "./getDataView";

export function getIsisMoveRowMenuVisible(ctx: any) {
  return !!getDataView(ctx).orderProperty?.name;
}