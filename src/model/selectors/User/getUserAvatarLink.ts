import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";

export function getUserAvatarLink(ctx: any): string {
  return getWorkbenchLifecycle(ctx).userInfo?.avatarLink ?? "/img/unknown-avatar.png";
}