import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";

export function reloadScreen(ctx: any) {
  return function* reloadScreen() {
    yield* getFormScreenLifecycle(ctx).refreshSession();
  };
}
