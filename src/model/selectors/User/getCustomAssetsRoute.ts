import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";

export function getCustomAssetsRoute(ctx: any): string | undefined {
  return getWorkbenchLifecycle(ctx).customAssetsRoute;
}