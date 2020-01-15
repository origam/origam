import selectors from "model/selectors-tree";

const HANDLED = Symbol("_$ErrorHandled");

export function handleError(ctx: any) {
  return function* handleError(error: any) {
    console.log('HANDLE ERROR', error)
    if (error[HANDLED]) {
      return;
    }
    error[HANDLED] = true;
    yield* selectors.error.getDialogController(ctx).pushError(error);
  };
}
