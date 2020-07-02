import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";

export default {
  onCloseClick(ctx: any) {
    return flow(function* onCancelClick(event: any) {
      try {
        const lifecycle = getFormScreenLifecycle(ctx);
        yield* lifecycle.onWorkflowCloseClick(event);
      } catch (e) {
        yield* handleError(ctx)(e);
        throw e;
      }
    });
  },

  onRepeatClick(ctx: any) {
    return flow(function* onRepeatClick(event: any) {
      try {
        const lifecycle = getFormScreenLifecycle(ctx);
        yield* lifecycle.onWorkflowRepeatClick(event);
      } catch (e) {
        yield* handleError(ctx)(e);
        throw e;
      }
    });
  },
};
