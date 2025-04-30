import { action, observable } from "mobx";
import React from "react";
import {
  IDialogDimensions,
  IDialogInfo,
  IDialogStackState
} from "src/dialog/types.ts";

let nextId = 0;

export class DialogStackState implements IDialogStackState {
  parent?: any;
  @observable.shallow accessor stackedDialogs: Array<IDialogInfo> = [];

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
      dimensions: dialogDimensions
    });
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