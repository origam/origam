import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getApi } from "model/selectors/getApi";
import { getSessionId } from "model/selectors/getSessionId";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export function onWorkflowNextClick(ctx: any) {
  return flow(function* onWorkflowNextClick(event: any) {
    try {
      const api = getApi(ctx);
      yield api.workflowNextQuery({ sessionFormIdentifier: getSessionId(ctx) });
      const uiResult = yield api.workflowNext({
        sessionFormIdentifier: getSessionId(ctx),
        CachedFormIds: [],
      });
      const lifecycle = getFormScreenLifecycle(ctx);
      lifecycle.killForm();
      yield* lifecycle.start(uiResult);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
