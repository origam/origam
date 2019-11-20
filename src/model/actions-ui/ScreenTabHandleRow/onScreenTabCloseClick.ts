import { flow } from "mobx";
import { closeForm } from "model/actions/closeForm";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";

export function onScreenTabCloseClick(ctx: any) {
  return flow(function* onFormTabCloseClick(event: any) {
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
  });
}
