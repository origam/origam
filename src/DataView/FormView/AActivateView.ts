import { action } from "mobx";
import { IViewType } from "../types/IViewType";
import { L } from "../../utils/types";
import { IRecCursor } from "../types/IRecCursor";
import { IASelProp } from "../types/IASelProp";
import { IAStartEditing } from "../types/IAStartEditing";
import { IAvailViews } from "../types/IAvailViews";
import { IAActivateView } from "../../Screens/types";


export class AActivateView implements IAActivateView{
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
    console.log('FormView - activate')
    availViews.setActiveView(IViewType.Form);

  }
}
