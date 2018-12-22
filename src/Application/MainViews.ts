import { action, computed, observable } from "mobx";
import { IAPI } from "../DataLoadingStrategy/types";
import { OpenedView } from "./OpenedView";
import { IMainViews, IOpenedView } from "./types";

let subidGen = 1;

export class MainViews implements IMainViews {
  constructor(public api: IAPI) {}

  @observable public activeView: IOpenedView | undefined = undefined;
  @observable public openedViewsState: IOpenedView[] = [];



  @action.bound
  public start() {
    return
  }

  @computed
  public get openedViews(): Array<{ order: number; view: IOpenedView }> {
    const viewMap = {};
    const result = [];
    for (const view of this.openedViewsState) {
      if (viewMap[view.id] === undefined) {
        viewMap[view.id] = 0;
      } else {
        viewMap[view.id]++;
      }
      result.push({ order: viewMap[view.id] as number, view });
    }
    return result;
  }

  @action.bound
  public closeView(id: string, subid: string) {
    const index = this.openedViewsState.findIndex(
      o => o.id === id && o.subid === subid
    );

    if (index > -1) {
      const oldView = this.openedViewsState[index];
      oldView.stop();
      if (
        !(
          this.activeView &&
          this.activeView.id === oldView.id &&
          this.activeView.subid === oldView.subid
        )
      ) {
        this.openedViewsState.splice(index, 1);
      } else if (this.openedViewsState.length === 1) {
        this.openedViewsState.splice(index, 1);
        this.activateView(undefined, undefined);
      } else {
        this.openedViewsState.splice(index, 1);
        const view = this.openedViewsState[Math.max(0, index - 1)];
        this.activateView(view.id, view.subid);
      }
    }
  }

  @action.bound
  public openOrActivateView(id: string, label: string) {
    let view = this.openedViewsState.find(o => o.id === id);
    if (!view) {
      view = this.openView(id, label);
    }
    this.activateView(id, view.subid);
  }

  @action.bound
  public activateView(id: string | undefined, subid: string | undefined) {
    if (!id || !subid) {
      this.activeView = undefined;
      return;
    }
    this.activeView = this.openedViewsState.find(
      o => o.id === id && o.subid === subid
    );
  }

  @action.bound
  public openView(id: string, label: string) {
    const view = new OpenedView(id, `${subidGen++}`, label, this.api);
    view.start();
    this.openedViewsState.push(view);
    this.activateView(view.id, view.subid);
    return view;
  }

  @action.bound
  public handleMenuFormItemClick(event: any, id: string, label: string) {
    if (event.ctrlKey) {
      this.openView(id, label);
    } else {
      this.openOrActivateView(id, label);
    }
  }
}
