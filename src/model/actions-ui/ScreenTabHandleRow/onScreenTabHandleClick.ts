import { flow } from "mobx";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { IOpenedScreen } from "model/entities/types/IOpenedScreen";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";

export function onScreenTabHandleClick(ctx: any) {
  return flow(function* onScreenTabHandleClick(
    event: any,
  ) {
    const screen = getOpenedScreen(ctx);
    yield* getWorkbenchLifecycle(ctx).onScreenTabHandleClick(event, screen);
  });
}
