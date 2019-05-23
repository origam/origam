import { observable, action, computed } from "mobx";
import { IPropCursor } from "./types/IPropCursor";

export class PropCursor implements IPropCursor {
  constructor(public P: {}) {}

  @observable selId: string | undefined;

  @action.bound setSelId(id: string | undefined) {
    console.log(id)
    this.selId = id;
  }

  @computed get isSelected() {
    return Boolean(this.selId);
  }
}
