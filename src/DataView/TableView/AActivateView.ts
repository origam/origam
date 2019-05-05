import { action } from "mobx";
import { IViewType } from "../types/IViewType";
import { L } from "../../utils/types";
import { IAActivateView } from "../types/IAActivateView";

export class AActivateView implements IAActivateView{
  constructor(
    public P: {
    }
  ) {}

  @action.bound
  do() {
    return
  }
}
