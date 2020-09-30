import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";

export function getShowChat(ctx: any): boolean {
  return getWorkbenchLifecycle(ctx).portalSettings?.showChat ?? false;
}