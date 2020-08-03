import {flow} from "mobx";
import {selectFirstRow} from "../../actions/DataView/selectFirstRow";
import {handleError} from "../../actions/handleError";

export function onFirstRowClick(ctx: any) {
  return flow(function* onPrevRowClick(event: any) {
    try {
      yield* selectFirstRow(ctx)();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
