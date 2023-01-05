import { FormFocusManager } from "model/entities/FormFocusManager";
import { GridFocusManager } from "model/entities/GridFocusManager";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { getActivePerspective } from "model/selectors/DataView/getActivePerspective";
import { IPanelViewType } from "model/entities/types/IPanelViewType";
import { getDataView } from "model/selectors/DataView/getDataView";

export class ScreenFocusManager {
  gridManagers: GridFocusManager[] = [];
  formManagers: FormFocusManager[] = [];

  registerGridFocusManager(manager:GridFocusManager) {
    this.gridManagers.push(manager);
  }
  registerFormFocusManager(manager:FormFocusManager) {
    this.formManagers.push(manager);
  }

  setFocus() {
    const managerWithOpenEditor =
      this.gridManagers.some(x => x.activeEditor) ||
      this.formManagers.some(x => x.lastFocused);
    if (!managerWithOpenEditor) {
      const formScreen = getFormScreen(this.parent);
      if (formScreen.rootDataViews.length === 1) {
        formScreen.rootDataViews[0].gridFocusManager.focusTableIfNeeded();
      }
    }
  }

  async activeEditorCloses(){
    for (const gridManager of this.gridManagers) {
      const dataView = getDataView(gridManager);
      if(dataView.isFormViewActive())
      {
        await dataView.formFocusManager.activeEditorCloses();
      }
      else if (dataView.isTableViewActive())
      {
        await dataView.gridFocusManager.activeEditorCloses();
      }
    }
  }

  parent: any;
}

