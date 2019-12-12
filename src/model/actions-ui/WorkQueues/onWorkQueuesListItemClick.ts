import { flow } from "mobx";
import { refreshWorkQueues } from "model/actions/WorkQueues/refreshWorkQueues";
import { handleError } from "model/actions/handleError";
import { getWorkQueues } from "model/selectors/WorkQueues/getWorkQueues";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";

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
