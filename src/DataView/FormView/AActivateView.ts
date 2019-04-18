import { action } from "mobx";
import { IViewType } from "../types/IViewType";
import { L } from "../../utils/types";
import { IRecCursor } from "../types/IRecCursor";
import { IASelProp } from "../types/IASelProp";
import { IAStartEditing } from "../types/IAStartEditing";
import { IAvailViews } from "../types/IAvailViews";


export class AActivateView {
  constructor(
    public P: {
      recCursor: L<IRecCursor>;
      aSelProp: L<IASelProp>;
      aStartEditing: L<IAStartEditing>;
      availViews: L<IAvailViews>;
    }
  ) {}

  @action.bound
  do() {
    const recCursor = this.P.recCursor();
    const aSelProp = this.P.aSelProp();
    const aStartEditing = this.P.aStartEditing();
    const availViews = this.P.availViews();
    // -------------------------------------------------------------
    availViews.setActiveView(IViewType.Form);
    if (recCursor.isSelected) {
      aSelProp.doSelFirst();
      aStartEditing.do();
    }
  }
}
