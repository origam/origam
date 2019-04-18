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
    this.availViews.setActiveView(viewType);
  }

  get availViews() {
    return unpack(this.P.availViews);
  }
}
