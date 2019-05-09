import { IADeactivateView } from "../types/IADeactivateView";
import { action } from "mobx";

export class ADeactivateView implements IADeactivateView {
  @action.bound
  do(): void {
    return;
  }
}
