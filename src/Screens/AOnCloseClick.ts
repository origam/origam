import { IAOnCloseClick, IACloseView } from "./types";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { action } from "mobx";

export class AOnCloseClick implements IAOnCloseClick {
  constructor(
    public P: {
      aCloseView: ML<IACloseView>;
    }
  ) {}

  @action.bound
  do(event: any, menuItemId: string, order: number): void {
    this.aCloseView.do(menuItemId, order);
  }

  get aCloseView() {
    return unpack(this.P.aCloseView);
  }
}
