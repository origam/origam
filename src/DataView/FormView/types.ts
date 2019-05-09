import { IUIFormRoot } from "../../presenter/view/Perspectives/FormView/types";
import { IDataView } from "../types/IDataView";
import { IViewType } from "../types/IViewType";
import { IPropReorder } from "../types/IPropReorder";
import { IPropCursor } from "../types/IPropCursor";
import { IForm } from "../types/IForm";
import { IAActivateView } from "../types/IAActivateView";
import { IADeactivateView } from "../types/IADeactivateView";
import { IASelNextProp } from "../types/IASelNextProp";
import { IASelPrevProp } from "../types/IASelPrevProp";
import { IASelProp } from "../types/IASelProp";

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
  form: IForm;
  aSelNextProp: IASelNextProp;
  aSelPrevProp: IASelPrevProp;
  aActivateView: IAActivateView;
  aDeactivateView: IADeactivateView;
  aSelProp: IASelProp;
}

