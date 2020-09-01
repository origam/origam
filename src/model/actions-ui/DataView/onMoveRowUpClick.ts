import { flow } from "mobx";
import { getDataView } from "../../selectors/DataView/getDataView";
import { handleError } from "../../actions/handleError";
import { getFormScreenLifecycle } from "../../selectors/FormScreen/getFormScreenLifecycle";

export function onMoveRowUpClick(ctx: any) {
  return flow(function* onMoveRowUpClick(event: any) {
    try {
      getDataView(ctx).moveSelectedRowUp();
      yield* getFormScreenLifecycle(ctx).onFlushData();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}

