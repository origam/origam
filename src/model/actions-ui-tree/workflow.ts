import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getApi } from "model/selectors/getApi";
import { getSessionId } from "model/selectors/getSessionId";

export default {
  onCancelClick(ctx: any) {
    return flow(function* onCancelClick(event: any) {
      try {
        const api = getApi(ctx);
        const sessionId = getSessionId(ctx);
        yield api.workflowAbort({ sessionFormIdentifier: sessionId });
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
        yield api.workflowRepeat({ sessionFormIdentifier: sessionId });
      } catch (e) {
        yield* handleError(ctx)(e);
        throw e;
      }
    });
  }
};
