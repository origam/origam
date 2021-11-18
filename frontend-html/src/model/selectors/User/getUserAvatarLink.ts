import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";

export function getUserAvatarLink(ctx: any): string | undefined {
  return getWorkbenchLifecycle(ctx).userInfo?.avatarLink;
}