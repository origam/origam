import { getApplicationLifecycle } from "../selectors/getApplicationLifecycle";

export function onLogoutClick(ctx: any) {
  getApplicationLifecycle(ctx).onSignOutClick({ event });
}
