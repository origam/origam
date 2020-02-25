import { flow } from "mobx";
import { getApplicationLifecycle } from "model/selectors/getApplicationLifecycle";
import { handleError } from "model/actions/handleError";
import { userManager } from "oauth";

export function onScreenToolbarLogoutClick(ctx: any) {
  return flow(function* onScreenToolbarLogoutClick(event: any) {
    try {
      for(let sessionStorageKey of Object.keys(window.sessionStorage)) {
        if(sessionStorageKey.startsWith('oidc.user')) {
          window.sessionStorage.removeItem("origamAuthToken");
          userManager.signoutRedirect();
          return
        }
      }
      yield* getApplicationLifecycle(ctx).onSignOutClick({ event });
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
