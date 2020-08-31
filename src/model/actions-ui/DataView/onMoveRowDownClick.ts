import { flow } from "mobx";
import { handleError } from "../../actions/handleError";

export function onMoveRowDownClick(ctx: any) {
  return flow(function* onMoveRowDownClick(event: any) {
    try {
      // getDataView(ctx).moveSelectedRowUp();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}