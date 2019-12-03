import { flow } from "mobx";
import { getApplicationLifecycle } from "model/selectors/getApplicationLifecycle";
import { handleError } from "model/actions/handleError";

export function onScreenToolbarLogoutClick(ctx: any) {
  return flow(function* onScreenToolbarLogoutClick(event: any) {
    try {
      yield* getApplicationLifecycle(ctx).onSignOutClick({ event });
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
