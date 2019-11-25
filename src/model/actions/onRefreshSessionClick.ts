import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { flow } from "mobx";
// TODO: Move to ui actions
export function onRefreshSessionClick(ctx: any) {
  return flow(function* onRefreshSessionClick() {
    yield* getFormScreenLifecycle(ctx).onRequestScreenReload();
  });
}
