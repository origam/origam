import { IViewType } from "./IViewType";

export interface IASwitchView {
  do(viewType: IViewType): void;
}
