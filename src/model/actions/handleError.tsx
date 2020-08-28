import selectors from "model/selectors-tree";
import {stopWorkQueues} from "./WorkQueues/stopWorkQueues";
import {performLogout} from "./User/performLogout";
import {T} from "utils/translation";

const HANDLED = Symbol("_$ErrorHandled");

export function handleError(ctx: any) {
  return function* handleError(error: any) {
    if (error.response && error.response.status === 401) {
      yield* stopWorkQueues(ctx)();
      selectors.error.getDialogController(ctx).dismissErrors();
      yield* selectors.error.getDialogController(ctx)
        .pushError(T(`Your request is no longer authorized, which means that 
          you were either logged out or your session expired. Please log in again.`,
            "request_no_longer_authorized"));
      yield* performLogout(ctx)();
      return;
    } else {
      console.log("HANDLE ERROR", error);
      if (error[HANDLED]) {
        return;
      }
      error[HANDLED] = true;
      yield* selectors.error.getDialogController(ctx).pushError(error);
    }
  };
}
