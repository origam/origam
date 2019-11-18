import { selectNextRow } from "model/actions/DataView/selectNextRow";

export function onNextRowClick(ctx: any) {
  return function onNextRowClick(event: any) {
    selectNextRow(ctx)();
  }
}