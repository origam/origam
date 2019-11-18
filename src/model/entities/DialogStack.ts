import { observable, action, computed } from "mobx";
import { IDialogStack } from "./types/IDialogStack";
import { IDialogDimensions } from "../../gui/Components/Dialog/types";

let nextId = 0;

export class DialogStack implements IDialogStack {
  parent?: any;
  @observable.shallow stackedDialogs: Array<{
    key: string;
    component: React.ReactElement;
    dimensions?: IDialogDimensions;
  }> = [];

  @computed get isAnyDialogShown() {
    return this.stackedDialogs.length > 0;
  }

  @action.bound pushDialog(
    key: string,
    component: React.ReactElement,
    dimensions?: IDialogDimensions
  ) {
    const useKey = key ? key : `DEFAULT_DIALOG_KEY_${nextId++}`;
    this.stackedDialogs.push({ key: useKey, component, dimensions });
    return () => {
      this.closeDialog(useKey);
    };
  }

  @action.bound closeDialog(key: string) {
    const index = this.stackedDialogs.findIndex(dlg => dlg.key === key);
    if (index > -1) {
      this.stackedDialogs.splice(index, 1);
    }
  }
}
