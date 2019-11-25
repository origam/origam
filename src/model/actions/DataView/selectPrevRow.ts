import { getDataView } from "model/selectors/DataView/getDataView";

export function selectPrevRow(ctx: any) {
  return function* selectPrevRow() {
    getDataView(ctx).selectPrevRow();
  };
}
