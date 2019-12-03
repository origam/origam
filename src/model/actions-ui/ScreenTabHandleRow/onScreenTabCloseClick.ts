import { flow } from "mobx";
import { closeForm } from "model/actions/closeForm";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";
import { handleError } from "model/actions/handleError";

export function onScreenTabCloseClick(ctx: any) {
  return flow(function* onFormTabCloseClick(event: any) {
    try {
      event.stopPropagation();
      // TODO: Wait for other async operation to finish?
      const openedScreen = getOpenedScreen(ctx);
      // TODO: Better lifecycle handling
      if (!openedScreen.content.isLoading) {
        const lifecycle = getFormScreenLifecycle(
          openedScreen.content.formScreen!
        );
        yield* lifecycle.onRequestScreenClose();
      } else {
        yield* closeForm(ctx)();
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
