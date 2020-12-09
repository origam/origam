import { getDataView } from "./getDataView";

export function getIsMoveRowMenuVisible(ctx: any) {
  const { orderProperty } = getDataView(ctx);
  return !!orderProperty && !orderProperty?.readOnly;
}
