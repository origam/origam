import { getDataView } from "./getDataView";
export function getSelectedRow(ctx: any) {
  return getDataView(ctx).selectedRow;
}
