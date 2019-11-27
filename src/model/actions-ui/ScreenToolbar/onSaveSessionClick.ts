import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { flow } from "mobx";
// TODO: Move to ui actions
export function onSaveSessionClick(ctx: any) {
  return flow(function* onSaveSessionClick() {
    yield* getFormScreenLifecycle(ctx).onSaveSession();
  });
}
