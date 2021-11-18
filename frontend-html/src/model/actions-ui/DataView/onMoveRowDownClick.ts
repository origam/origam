import { flow } from "mobx";
import { handleError } from "../../actions/handleError";
import {getDataView} from "../../selectors/DataView/getDataView";
import {getFormScreenLifecycle} from "../../selectors/FormScreen/getFormScreenLifecycle";

export function onMoveRowDownClick(ctx: any) {
  return flow(function* onMoveRowDownClick(event: any) {
    try {
      getDataView(ctx).moveSelectedRowDown();
      yield* getFormScreenLifecycle(ctx).onFlushData();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}