import { getWorkQueues } from "model/selectors/WorkQueues/getWorkQueues";

export function refreshWorkQueues(ctx: any) {
  return function*refreshWorkQueues() {
    yield* getWorkQueues(ctx).getWorkQueueList();
  }
}