import { errDialogPromise } from "model/entities/ErrorDialog";

const HANDLED = Symbol("_$ErrorHandled");

export function handleError(ctx: any) {
  return function* handleError(error: any) {
    if (error[HANDLED]) {
      return;
    }
    error[HANDLED] = true;
    yield errDialogPromise(ctx)(error);
  };
}
