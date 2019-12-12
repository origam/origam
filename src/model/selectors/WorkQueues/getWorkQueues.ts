import { getWorkbench } from "../getWorkbench";

export function getWorkQueues(ctx: any) {
  return getWorkbench(ctx).workQueues;
}