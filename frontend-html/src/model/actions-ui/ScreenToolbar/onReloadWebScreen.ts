import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import {getWebScreen} from "model/selectors/WebScreen/getWebScreen";

// TODO: Move to ui actions
export function onReloadWebScreenClick(ctx: any) {
  return flow(function* onReloadWebScreenClick() {
    try {
      getWebScreen(ctx).reload();
      //yield* getFormScreenLifecycle(ctx).onRequestScreenReload();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
