import {flow} from "mobx";
import {selectLastRow} from "../../actions/DataView/selectLastRow";
import {handleError} from "../../actions/handleError";

export function onLastRowClick(ctx: any) {
  return flow(function* onLastRowClick(event: any) {
    try {
      yield* selectLastRow(ctx)();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}