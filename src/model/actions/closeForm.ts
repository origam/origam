import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";

export function closeForm(ctx: any) {
  return function* closeForm(): Generator {
    const lifecycle = getWorkbenchLifecycle(ctx);
    const openedScreen = getOpenedScreen(ctx);
    yield* lifecycle.closeForm(openedScreen);
  };
}
