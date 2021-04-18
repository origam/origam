import { flushCurrentRowData } from "../../../actions/DataView/TableView/flushCurrentRowData";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { crs_fieldBlur_ActionClick } from "model/actions/actionSync";

export function onFieldBlur(ctx: any) {
  return flow(function* onFieldBlur(event: any) {
    try {
      yield* crs_fieldBlur_ActionClick.runGenerator(function*() {
        console.log("EDITOR BLUR:", ctx);
        yield* flushCurrentRowData(ctx)();
        console.log("BLUR EVENT HANDLER FINISHED");
      })
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
