import { action } from "mobx";
import { IViewType } from "../types/IViewType";
import { L, ML } from "../../utils/types";
import { IRecCursor } from "../types/IRecCursor";
import { IASelProp } from "../types/IASelProp";
import { IAStartEditing } from "../types/IAStartEditing";
import { IAvailViews } from "../types/IAvailViews";
import { IAActivateView } from "../../Screens/types";
import { IFormViewMachine } from "./types";
import { unpack } from "../../utils/objects";

export class AActivateView implements IAActivateView {
  constructor(
    public P: {
      recCursor: IRecCursor;
      aSelProp: IASelProp;
      aStartEditing: IAStartEditing;
      availViews: IAvailViews;
      machine: IFormViewMachine;
    }
  ) {}

  @action.bound
  do() {
    const recCursor = this.P.recCursor;
    const aSelProp = this.P.aSelProp;
    const aStartEditing = this.P.aStartEditing;
    const availViews = this.P.availViews;
    // -------------------------------------------------------------
    console.log("FormView - activate");
    availViews.setActiveView(IViewType.Form);
    this.machine.start();
  }

  get machine() {
    return unpack(this.P.machine);
  }
}
