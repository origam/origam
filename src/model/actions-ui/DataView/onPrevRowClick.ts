import { selectPrevRow } from "model/actions/DataView/selectPrevRow";
import { flow } from "mobx";

export function onPrevRowClick(ctx: any) {
  return flow(function* onPrevRowClick(event: any) {
    selectPrevRow(ctx)();
  });
}
