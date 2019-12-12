import { getWorkQueues } from "./getWorkQueues";

export function getWorkQueuesItems(ctx: any) {
  return getWorkQueues(ctx).items;
}