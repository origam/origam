import { action } from "mobx";
import { IFormActions, IFormState } from "./types";

export class FormActions implements IFormActions {
  constructor(public state: IFormState) {}

  @action.bound
  public focusRoot(): void {
    this.state.elmRoot && this.state.elmRoot.focus();
  }

  @action.bound
  public refRoot(element: HTMLDivElement): void {
    this.state.setRefRoot(element);
  }
}
