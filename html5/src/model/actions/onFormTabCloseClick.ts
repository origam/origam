import { closeForm } from "./closeForm";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";
import { isILoadedFormScreen } from '../entities/types/IFormScreen';

export function onFormTabCloseClick(ctx: any) {
  return function onFormTabCloseClick(event: any) {
    // TODO: Wait for other async operation to finish?
    const openedScreen = getOpenedScreen(ctx);
    if(isILoadedFormScreen(openedScreen.content)) {
      const lifecycle = getFormScreenLifecycle(openedScreen.content);
      lifecycle.onRequestScreenClose();
    } else {
      closeForm(ctx);
    }
  }
}