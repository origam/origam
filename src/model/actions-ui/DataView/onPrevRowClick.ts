import { selectPrevRow } from "model/actions/DataView/selectPrevRow";

export function onPrevRowClick(ctx: any) {
  return function onPrevRowClick(event: any) {
    selectPrevRow(ctx)();
  }
}