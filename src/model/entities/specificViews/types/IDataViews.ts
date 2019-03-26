import { IViewType } from "./IViewType";
import { IDataView } from "./IDataView";

export interface IDataViews {
  id: string;
  byType(type: IViewType): IDataView | undefined;
  activateView(viewId: string): void;
}
