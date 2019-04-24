import { IUIFormRoot } from "../../presenter/view/Perspectives/FormView/types";
import { IDataView } from "../types/IDataView";
import { IViewType } from "../types/IViewType";
import { IPropReorder } from "../types/IPropReorder";
import { IPropCursor } from "../types/IPropCursor";

export function isFormView(obj: any): obj is IFormView {
  return obj.type ===  IViewType.Form;
}

export interface IFormView {
  type: IViewType.Form;
  init(): void;
  uiStructure: IUIFormRoot[];
  dataView: IDataView;
  propReorder: IPropReorder;
  propCursor: IPropCursor;
}