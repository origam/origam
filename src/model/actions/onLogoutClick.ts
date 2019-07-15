import { getApplicationLifecycle } from "../selectors/getApplicationLifecycle";

export function onLogoutClick(ctx: any, event: any) {
  getApplicationLifecycle(ctx).onSignOutClick({ event });
}
