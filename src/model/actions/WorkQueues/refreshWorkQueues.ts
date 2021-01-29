import { getSearcher } from "model/selectors/getSearcher";
import {getWorkQueues} from "model/selectors/WorkQueues/getWorkQueues";

export function refreshWorkQueues(ctx: any) {
  return function*refreshWorkQueues() {
    const workQueues = getWorkQueues(ctx);
    yield* workQueues.getWorkQueueList();
    getSearcher(ctx).indexWorkQueues(workQueues.items);
  }
}