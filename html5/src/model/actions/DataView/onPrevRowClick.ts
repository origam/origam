import { selectPrevRow } from "./selectPrevRow";

export function onPrevRowClick(ctx: any) {
  return function onPrevRowClick(event: any) {
    selectPrevRow(ctx)();
  };
}
