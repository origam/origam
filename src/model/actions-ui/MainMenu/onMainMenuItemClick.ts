import { flow } from "mobx";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { handleError } from "model/actions/handleError";

export function onMainMenuItemClick(ctx: any) {
  return flow(function* onMainMenuItemClick(args: {
    event: any;
    item: any;
    idParameter: string | undefined;
    isSingleRecordEdit?: boolean;
  }) {
    try {
      yield* getWorkbenchLifecycle(ctx).onMainMenuItemClick(args);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
