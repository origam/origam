import { closeForm } from "./closeForm";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";

export function onFormTabCloseClick(ctx: any) {
  return function onFormTabCloseClick(event: any) {
    event.stopPropagation();
    // TODO: Wait for other async operation to finish?
    const openedScreen = getOpenedScreen(ctx);
    // TODO: Better lifecycle handling
    if(!openedScreen.content.isLoading) {
      const lifecycle = getFormScreenLifecycle(openedScreen.content.formScreen!);
      lifecycle.onRequestScreenClose();
    } else {
      closeForm(ctx)();
    }
  }
}