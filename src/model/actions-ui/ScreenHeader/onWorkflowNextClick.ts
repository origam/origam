import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getApi } from "model/selectors/getApi";
import { getSessionId } from "model/selectors/getSessionId";

export function onWorkflowNextClick(ctx: any) {
  return flow(function* onWorkflowNextClick(event: any) {
    try {
      const api = getApi(ctx);
      yield api.workflowNextQuery({ sessionFormIdentifier: getSessionId(ctx) });
      yield api.workflowNext({ sessionFormIdentifier: getSessionId(ctx), CachedFormIds: [] });
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
