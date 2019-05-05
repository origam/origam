import { IADeactivateView } from "../types/IADeactivateVIew";
import { action } from "mobx";

export class ADeactivateView implements IADeactivateView {
  @action.bound
  do(): void {
    return;
  }
}
