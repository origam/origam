import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";

export function getLoggedUserName(ctx: any): string | undefined {
  return getWorkbenchLifecycle(ctx).userInfo?.userName;
}
