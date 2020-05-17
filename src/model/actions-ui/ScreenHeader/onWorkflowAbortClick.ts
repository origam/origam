import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getApi } from "model/selectors/getApi";
import { getSessionId } from "model/selectors/getSessionId";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export function onWorkflowAbortClick(ctx: any) {
  return flow(function* onWorkflowAbortClick(event: any) {
    try {
      const lifecycle = getFormScreenLifecycle(ctx);
      yield* lifecycle.onWorkflowAbortClick(event);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
