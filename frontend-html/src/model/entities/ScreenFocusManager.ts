import { FormFocusManager } from "model/entities/FormFocusManager";
import { GridFocusManager } from "model/entities/GridFocusManager";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";

export class ScreenFocusManager {
  gridManagers: GridFocusManager[] = [];

  registerGridFocusManager(id: string, manager:GridFocusManager) {
    this.gridManagers.push(manager);
  }

  setFocus() {
    const managerWithOpenEditor = this.gridManagers.some(x => x.activeEditor);
    if (!managerWithOpenEditor) {
      const formScreen = getFormScreen(this.parent);
      if (formScreen.rootDataViews.length === 1) {
        formScreen.rootDataViews[0].gridFocusManager.focusTableIfNeeded();
      }
    }
  }

  parent: any;
}

