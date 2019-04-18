import { action } from "mobx";
import { IViewType } from "../types/IViewType";
import { L } from "../../utils/types";

interface IAvailViews {
  setActiveView(viewType: IViewType): void;
}

export class AActivateView {
  constructor(
    public P: {
      availViews: L<IAvailViews>;
    }
  ) {}

  @action.bound
  do() {
    const availViews = this.P.availViews();
    // -------------------------------------------------------------
    availViews.setActiveView(IViewType.Form);
  }
}
