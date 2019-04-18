import { observable, action, computed } from "mobx";
import { IRecCursor } from "./types/IRecCursor";


export class RecCursor implements IRecCursor {
  constructor(public P: {}) {}

  @observable selId: string | undefined;

  @action.bound setSelId(id: string) {
    console.log(id)
    this.selId = id;
  }

  @computed get isSelected() {
    return Boolean(this.selId);
  }
}
