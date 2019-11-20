import { flow } from "mobx";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";

export function onMainMenuItemClick(ctx: any) {
  return flow(function*onMainMenuItemClick(args: {event: any, item: any}) {
    yield* getWorkbenchLifecycle(ctx).onMainMenuItemClick(args)
  })
}