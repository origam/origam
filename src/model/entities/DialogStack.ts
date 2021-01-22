import {action, computed, observable} from "mobx";
import {IDialogStack} from "./types/IDialogStack";
import { IDialogInfo } from "./types/IDialogInfo";
import {IDialogDimensions} from "../../gui/Components/Dialog/types";

let nextId = 0;

export class DialogStack implements IDialogStack {
  parent?: any;
  @observable.shallow stackedDialogs: Array<IDialogInfo> = [];

  @computed get isAnyDialogShown() {
    return this.stackedDialogs.length > 0;
  }

  @action.bound pushDialog(    
    key: string,
    component: React.ReactElement,
    dialogDimensions?: IDialogDimensions,
    closeOnClickOutside?: boolean) {
    const useKey = key ? key : `DEFAULT_DIALOG_KEY_${nextId++}`;
    this.stackedDialogs.push({
      key: useKey,
      closeOnClickOutside: closeOnClickOutside,
      component: component,
      dimensions: dialogDimensions });
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

  isOpen(key: string) {
    return this.stackedDialogs.findIndex(info => info.key === key) !== -1;
  }
}
