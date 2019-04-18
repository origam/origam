import { IAOnHandleClick, IAActivateView } from "./types";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { action } from "mobx";

export class AOnHandleClick implements IAOnHandleClick {
  constructor(
    public P: {
      aActivateView: ML<IAActivateView>;
    }
  ) {}

  @action.bound
  do(event: any, menuItemId: string, order: number): void {
    this.aActivateView.do(menuItemId, order);
  }

  get aActivateView() {
    return unpack(this.P.aActivateView);
  }
}
