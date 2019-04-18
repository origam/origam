import { action, observable } from "mobx";
import { IAnouncer } from "./types/IAnouncer";

export class Anouncer implements IAnouncer {

  constructor(public P: {}) {}

  @observable message: string | undefined;
  
  @action.bound inform(message: string) {
    this.message = message;
    return this.resetInform;
  }

  @action.bound
  resetInform(): void {
    this.message = undefined
  }
}