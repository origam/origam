import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
// TODO: Move to ui actions
export function onRefreshSessionClick(ctx: any) {
  return flow(function* onRefreshSessionClick() {
    try {
      yield* getFormScreenLifecycle(ctx).onRequestScreenReload();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
