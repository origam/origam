import { IFormView, IUIFormRoot } from "./types";
import { IViewType } from "../../../../DataView/types/IViewType";
import { ML } from "../../../../utils/types";
import { IToolbar } from "../types";
import { unpack } from "../../../../utils/objects";

export class FormViewPresenter implements IFormView {
  constructor(
    public P: {
      toolbar: ML<IToolbar | undefined>;
      uiStructure: ML<IUIFormRoot[]>;
    }
  ) {}

  type: IViewType.Form = IViewType.Form;
  fields = new Map();

  get toolbar() {
    return unpack(this.P.toolbar);
  }

  get uiStructure() {
    return unpack(this.P.uiStructure);
  }
}
