import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
// TODO: Move to ui actions
export function onRefreshSessionClick(ctx: any) {
  return function onRefreshSessionClick() {
    getFormScreenLifecycle(ctx).onRequestScreenReload();
  }
}