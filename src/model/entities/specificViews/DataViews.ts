import { observable, computed, action } from "mobx";
import { IViewType } from "./types/IViewType";
import { IDataViews } from "./types/IDataViews";
import { IDataView } from "./types/IDataView";

export class DataViews implements IDataViews {
  constructor(
    public id: string,
    availableViews: IDataView[],
    initialActiveView: IViewType
  ) {
    this.availableViews = availableViews;
    this.activeViewType = initialActiveView;
  }

  availableViews: IDataView[];
  @observable activeViewType: IViewType | undefined;

  byType(type: IViewType): IDataView | undefined {
    return this.availableViews.find(av => av.type === type);
  }

  @computed get activeView() {
    return this.activeViewType
      ? this.getViewByType(this.activeViewType)
      : undefined;
  }

  getViewByType(viewType: string) {
    return this.availableViews.find(view => view.type === viewType);
  }

  @action.bound
  activateView(viewType: IViewType): void {
    // this.activeView && this.activeView.deactivateView();
    // const newView = this.getViewByType(viewType);
    // newView && newView.activateView();
    this.activeViewType = viewType;
  }
}
