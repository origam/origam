import { observable, action } from "mobx";
import { IDialogStack } from "./types/IDialogStack";
import { IDialogDimensions } from "../../gui/Components/Dialog/types";

export class DialogStack implements IDialogStack {
  @observable.shallow stackedDialogs: Array<{
    key: string;
    component: React.ReactElement;
    dimensions?: IDialogDimensions;
  }> = [];

  @action.bound pushDialog(
    key: string,
    component: React.ReactElement,
    dimensions?: IDialogDimensions
  ) {
    this.stackedDialogs.push({ key, component, dimensions });
  }

  @action.bound closeDialog(key: string) {
    const index = this.stackedDialogs.findIndex(dlg => dlg.key === key);
    if (index > -1) {
      this.stackedDialogs.splice(index, 1);
    }
  }
}
