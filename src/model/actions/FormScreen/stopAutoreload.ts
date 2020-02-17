import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle"

export function stopAutoreload(ctx: any) {
  return function*() {
    getFormScreenLifecycle(ctx).clearAutorefreshInterval();
  }
}