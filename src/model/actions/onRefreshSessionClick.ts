import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export function onRefreshSessionClick(ctx: any) {
  return function onRefreshSessionClick() {
    getFormScreenLifecycle(ctx).onRefreshSession();
  }
}