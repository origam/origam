import { selectNextRow } from "model/actions/DataView/selectNextRow";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";

export function onNextRowClick(ctx: any) {
  return flow(function* onNextRowClick(event: any) {
    try {
      yield* selectNextRow(ctx)();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
