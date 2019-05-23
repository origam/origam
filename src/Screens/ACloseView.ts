import { IACloseView, IMainViews, IAActivateView } from "./types";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { action } from "mobx";
import * as ScreensActions from "./ScreensActions";

export class ACloseView implements IACloseView {
  constructor(
    public P: {
      mainViews: ML<IMainViews>;
      aActivateView: ML<IAActivateView>;
    }
  ) {}

  @action.bound
  do(id: string, order: number): void {
    const view = this.mainViews.findView(id, order);
    // view && view.deactivate();
    // view && view.close();
    view && view.dispatch(ScreensActions.stopScreen());
    if (
      this.mainViews.activeViewId === id &&
      this.mainViews.activeViewOrder === order
    ) {
      // Closing currently active tab.
      const closest = this.mainViews.findClosest(id, order);
      if (closest) {
        this.aActivateView.do(closest.menuItemId, closest.order);
      }
    }

    this.mainViews.deleteView(id, order);
  }

  get mainViews() {
    return unpack(this.P.mainViews);
  }

  get aActivateView() {
    return unpack(this.P.aActivateView);
  }
}
