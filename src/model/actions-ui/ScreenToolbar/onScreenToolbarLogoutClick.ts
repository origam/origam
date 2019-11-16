import { flow } from "mobx";
import { getApplicationLifecycle } from "model/selectors/getApplicationLifecycle";

export function onScreenToolbarLogoutClick(ctx: any) {
  return flow(function*onScreenToolbarLogoutClick(event: any) {
    yield getApplicationLifecycle(ctx).onSignOutClick({ event });
  });
}