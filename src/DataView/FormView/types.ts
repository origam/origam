import { IUIFormRoot } from "../../presenter/view/Perspectives/FormView/types";
import { IDataView } from "../types/IDataView";
import { IViewType } from "../types/IViewType";

export function isFormView(obj: any): obj is IFormView {
  return obj.type ===  IViewType.Form;
}

export interface IFormView {
  type: IViewType.Form;
  init(): void;
  uiStructure: IUIFormRoot[];
  dataView: IDataView;
}