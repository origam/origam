import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
// TODO: Move to ui actions
export function onSaveSessionClick(ctx: any) {
  return flow(function* onSaveSessionClick() {
    try {
      yield* getFormScreenLifecycle(ctx).onSaveSession();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
