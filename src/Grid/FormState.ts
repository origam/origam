import { IFormState } from "./types";
import { observable } from "mobx";

export class FormState implements IFormState {
  @observable.ref
  public elmRoot: HTMLDivElement | null = null;

  public setRefRoot(element: HTMLDivElement): void {
    this.elmRoot = element;
  }
}
