import {getWorkQueues} from "./getWorkQueues";

export function getWorkQueuesTotalItemsCount(ctx: any) {
  return getWorkQueues(ctx).totalItemCount;
}