import { IAActivateView, IMainViews } from "./types";
import { ML } from '../utils/types';
import { unpack } from "../utils/objects";
import { action } from "mobx";

export class AActivateView implements IAActivateView {
  constructor(
    public P: {
      mainViews: ML<IMainViews>;
    }
  ) {}

  @action.bound
  do(id: string, order: number): void {
    this.mainViews.activateView(id, order);
    // const view = this.mainViews.findView(id, order);
    // view && view.activate();
  }

  get mainViews() {
    return unpack(this.P.mainViews);
  }

}