import { ISpecificViews } from "./types/ISpecificViews";
import { ISpecificView } from "./types/ISpecificView";
import { observable, computed, action } from "mobx";

interface ISpecificViewsParam {
  availableViews: ISpecificView[];
}

export class SpecificViews implements ISpecificViews {
  constructor(param: ISpecificViewsParam) {
    this.availableViews = param.availableViews;
  }
  
  availableViews: ISpecificView[];
  @observable activeViewType: string | undefined;

  @computed get activeView() {
    return this.activeViewType
      ? this.getViewByType(this.activeViewType)
      : undefined;
  }

  getViewByType(viewType: string) {
    return this.availableViews.find(view => view.type === viewType);
  }

  @action.bound
  activateView(viewType: string): void {
    this.activeView && this.activeView.deactivateView();
    const newView = this.getViewByType(viewType);
    newView && newView.activateView();
    this.activeViewType = viewType;
  }
}
