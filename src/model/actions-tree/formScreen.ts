import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle"

export default {
  refresh(ctx: any) {
    return function*refresh() {
      yield* getFormScreenLifecycle(ctx).refreshSession();
    }
  }
}