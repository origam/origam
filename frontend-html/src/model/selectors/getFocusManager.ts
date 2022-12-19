import { ScreenFocusManager } from "model/entities/ScreenFocusManager";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";

export function getFocusManager(ctx: any): ScreenFocusManager {
  const workbench = getFormScreen(ctx)
  return workbench.focusManager;
}