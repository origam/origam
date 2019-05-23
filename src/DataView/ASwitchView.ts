import { IASwitchView } from "./types/IASwitchView";

import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { action } from "mobx";
import { IAvailViews } from "./types/IAvailViews";
import { IViewType } from "./types/IViewType";
import { activateView, deactivateView } from "./DataViewActions";

export class ASwitchView implements IASwitchView {
  constructor(
    public P: { dispatch: (event: any) => void /*availViews: ML<IAvailViews>*/ }
  ) {}

  @action.bound
  do(viewType: IViewType): void {
    this.P.dispatch(deactivateView());
    this.P.dispatch(activateView({ viewType }));
  }

}
