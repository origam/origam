import { IViewType } from "./types/IViewType";
import { observable, action, computed } from "mobx";
import { L, ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { IAvailViews, IView } from "./types/IAvailViews";
import { ISpecificView } from './types/ISpecificView';



export class AvailViews implements IAvailViews {
  constructor(
    public P: {
      items: ML<ISpecificView[]>;
      initialActiveViewType: ML<IViewType | undefined>;
    }
  ) {
    this.activeViewType = unpack(P.initialActiveViewType);
  }

  @observable activeViewType: IViewType | undefined;

  @computed get activeView(): ISpecificView | undefined {
    return this.items.find(item => item.type === this.activeViewType);
  }

  @action.bound setActiveView(viewType: IViewType) {
    this.activeViewType = viewType;
  }

  get items() {
    return unpack(this.P.items);
  }
}
