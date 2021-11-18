import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";

export function getShowWorkQues(ctx: any): boolean {
  return getWorkbenchLifecycle(ctx).portalSettings?.showWorkQueues ?? false;
}