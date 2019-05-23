import { IViewType } from "./IViewType";
import { ISpecificView } from "./ISpecificView";

export interface IView {
  type: IViewType;
}

export interface IAvailViews {
  activeViewType: IViewType | undefined;
  activeView: ISpecificView | undefined;
  items: ISpecificView[];
  // setActiveView(viewType: IViewType): void;
}
