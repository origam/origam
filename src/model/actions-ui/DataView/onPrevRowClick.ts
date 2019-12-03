import { selectPrevRow } from "model/actions/DataView/selectPrevRow";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";

export function onPrevRowClick(ctx: any) {
  return flow(function* onPrevRowClick(event: any) {
    try {
      yield* selectPrevRow(ctx)();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
