import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";

export function onWorkQueuesListItemClick(ctx: any) {
  return flow(function* onWorkQueuesListItemClick(event: any, item: any) {
    try {
      yield* getWorkbenchLifecycle(ctx).onWorkQueueListItemClick(event, item);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
