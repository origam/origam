import { observable, action } from "mobx";
import { IDialogStack } from "./types/IDialogStack";

export class DialogStack implements IDialogStack {
  @observable.shallow stackedDialogs: Array<{
    key: string;
    component: React.ReactElement;
  }> = [];

  @action.bound pushDialog(key: string, component: React.ReactElement) {
    this.stackedDialogs.push({ key, component });
  }

  @action.bound closeDialog(key: string) {
    const index = this.stackedDialogs.findIndex(dlg => dlg.key === key);
    if (index > -1) {
      this.stackedDialogs.splice(index, 1);
    }
  }
}
