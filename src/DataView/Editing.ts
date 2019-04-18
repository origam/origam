import { observable, action } from "mobx";
import { IEditing } from "./types/IEditing";

export class Editing implements IEditing {
  constructor(public P: {}) {}

  @observable isEditing = false;

  @action.bound setEditing(state: boolean) {
    this.isEditing = state;
  }
}
