import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getApi } from "model/selectors/getApi";
import { getSessionId } from "model/selectors/getSessionId";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export default {
  onCancelClick(ctx: any) {
    return flow(function* onCancelClick(event: any) {
      try {
        const api = getApi(ctx);
        const sessionId = getSessionId(ctx);
        const uiResult = yield api.workflowAbort({ sessionFormIdentifier: sessionId });
        const lifecycle = getFormScreenLifecycle(ctx);
        lifecycle.killForm();
        yield* lifecycle.start(uiResult);
      } catch (e) {
        yield* handleError(ctx)(e);
        throw e;
      }
    });
  },

  onRepeatClick(ctx: any) {
    return flow(function* onRepeatClick(event: any) {
      try {
        const api = getApi(ctx);
        const sessionId = getSessionId(ctx);
        const uiResult = yield api.workflowRepeat({ sessionFormIdentifier: sessionId });
        const lifecycle = getFormScreenLifecycle(ctx);
        lifecycle.killForm();
        yield* lifecycle.start(uiResult);
      } catch (e) {
        yield* handleError(ctx)(e);
        throw e;
      }
    });
  },
};
