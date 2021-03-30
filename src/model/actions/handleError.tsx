import selectors from "model/selectors-tree";
import {stopWorkQueues} from "./WorkQueues/stopWorkQueues";
import {performLogout} from "./User/performLogout";
import {T} from "utils/translation";
import { flow } from "mobx";

const HANDLED = Symbol("_$ErrorHandled");

export function handleError(ctx: any) {
  return function* handleError(error: any) {
    if (error.response && error.response.status === 474) {
      // 747 ~ ServerObjectDisposed happens when the user closes a form before all pending requests have
      // finished (RowStates for example)
      return;
    }
    if (error.response && error.response.status === 401) {
      yield* stopWorkQueues(ctx)();
      selectors.error.getDialogController(ctx).dismissErrors();
      yield* selectors.error.getDialogController(ctx).pushError(
        T(
          `Your request is no longer authorized, which means that 
          you were either logged out or your session expired. Please log in again.`,
          "request_no_longer_authorized"
        )
      );
      yield* performLogout(ctx)();
      return;
    } else {
      if (error[HANDLED]) {
        yield error[HANDLED];
        return;
      }
      const promise = flow(() => selectors.error.getDialogController(ctx).pushError(error))()
      error[HANDLED] = promise;
      yield promise;
    }
  };
}
