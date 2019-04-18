import { observable, action, computed } from "mobx";
import { AActivateView } from "../DataView/FormView/AActivateView";
import { IMainViews, IMainView } from "./types";
import _ from "lodash";

export class MainViews implements IMainViews {
  constructor(public P: {}) {}

  @observable activeViewId: string | undefined = undefined;
  @observable activeViewOrder: number | undefined = undefined;
  @observable openedViews: IMainView[] = [];

  @computed get activeView(): IMainView | undefined {
    return this.openedViews.find(
      ov =>
        ov.menuItemId === this.activeViewId && ov.order === this.activeViewOrder
    );
  }

  isActiveView(view: IMainView): boolean {
    return (
      view.menuItemId === this.activeViewId &&
      view.order === this.activeViewOrder
    );
  }

  findView(id: string, order: number): IMainView | undefined {
    return this.openedViews.find(o => o.menuItemId === id && o.order === order);
  }

  findLastById(id: string): IMainView | undefined {
    return _.findLast(this.openedViews, o => o.menuItemId === id);
  }

  findFirstById(id: string): IMainView | undefined {
    return this.openedViews.find(o => o.menuItemId === id);
  }

  findClosest(id: string, order: number): IMainView | undefined {
    const idx = this.openedViews.findIndex(
      ov => ov.menuItemId === id && ov.order === order
    );
    if (idx > -1 && this.openedViews.length > 1) {
      let newIndex;
      if (idx > 0) {
        newIndex = idx - 1;
      } else {
        newIndex = idx + 1;
      }
      return this.openedViews[newIndex];
    } else {
      return;
    }
  }

  @action.bound
  pushView(view: IMainView): void {
    console.log(view);
    this.openedViews.push(view);
  }

  @action.bound deleteView(id: string, order: number) {
    const idx = this.openedViews.findIndex(
      ov => ov.menuItemId === id && ov.order === order
    );
    idx > -1 && this.openedViews.splice(idx, 1);
  }

  @action.bound
  activateView(id: string | undefined, order: number | undefined): void {
    this.activeViewId = id;
    this.activeViewOrder = order;
  }
}
