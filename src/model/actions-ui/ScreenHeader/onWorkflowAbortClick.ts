import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getApi } from "model/selectors/getApi";
import { getSessionId } from "model/selectors/getSessionId";

export function onWorkflowAbortClick(ctx: any) {
  return flow(function* onWorkflowAbortClick(event: any) {
    try {
      const api = getApi(ctx);
      yield api.workflowAbort({sessionFormIdentifier: getSessionId(ctx)});
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
