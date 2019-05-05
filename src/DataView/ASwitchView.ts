import { IASwitchView } from "./types/IASwitchView";

import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { action } from "mobx";
import { IAvailViews } from "./types/IAvailViews";
import { IViewType } from "./types/IViewType";

export class ASwitchView implements IASwitchView {
  constructor(public P: { availViews: ML<IAvailViews> }) {}

  @action.bound
  do(viewType: IViewType): void {
    if(this.availViews.activeView) {
      this.availViews.activeView.aDeactivateView.do();
    }
    this.availViews.setActiveView(viewType);
    if(this.availViews.activeView) {
      this.availViews.activeView.aActivateView.do();
    }
  }

  get availViews() {
    return unpack(this.P.availViews);
  }
}
