import { flow } from "mobx";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";
import { handleError } from "model/actions/handleError";

export function onScreenTabHandleClick(ctx: any) {
  return flow(function* onScreenTabHandleClick(event: any) {
    try {
      const screen = getOpenedScreen(ctx);
      yield* getWorkbenchLifecycle(ctx).onScreenTabHandleClick(event, screen);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
