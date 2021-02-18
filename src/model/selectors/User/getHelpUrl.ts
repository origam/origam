import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";

export function getHelpUrl(ctx: any): string | undefined {
  return getWorkbenchLifecycle(ctx).portalSettings?.helpUrl;
}