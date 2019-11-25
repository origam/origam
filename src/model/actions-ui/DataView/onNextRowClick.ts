import { selectNextRow } from "model/actions/DataView/selectNextRow";
import { flow } from "mobx";

export function onNextRowClick(ctx: any) {
  return flow(function* onNextRowClick(event: any) {
    yield* selectNextRow(ctx)();
  })
}