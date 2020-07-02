import {flushCurrentRowData} from "../../../actions/DataView/TableView/flushCurrentRowData";
import {flow} from "mobx";
import {handleError} from "model/actions/handleError";

export function onFieldBlur(ctx: any) {
  return flow(function* onFieldBlur(event: any) {
    try {
      yield* flushCurrentRowData(ctx)();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
