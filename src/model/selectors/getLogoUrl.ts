import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";

export function getLogoUrl(ctx: any): string | undefined {
  return getWorkbenchLifecycle(ctx).logoUrl;
}