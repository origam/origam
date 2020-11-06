import { flow } from "mobx";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { handleError } from "model/actions/handleError";

export function onMainMenuItemContextMenu(ctx: any) {
  return flow(function* onMainMenuItemContextMenu(args: {
    event: any;
    item: any;
  }) {
    try {
      yield* getWorkbenchLifecycle(ctx).onMainMenuItemContextMenu(args);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}