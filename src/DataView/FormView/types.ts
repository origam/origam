import { IUIFormRoot } from "../../presenter/view/Perspectives/FormView/types";
import { IViewType } from "../types/IViewType";
import { IPropReorder } from "../types/IPropReorder";
import { IPropCursor } from "../types/IPropCursor";
import { IForm } from "../types/IForm";
import { IAActivateView } from "../types/IAActivateView";
import { IADeactivateView } from "../types/IADeactivateView";
import { IASelNextProp } from "../types/IASelNextProp";
import { IASelPrevProp } from "../types/IASelPrevProp";
import { IASelProp } from "../types/IASelProp";
import { IDataViewMediator02 } from "../DataViewMediator02";
import { IFormViewMediator } from "./FormViewMediator";

export function isFormView(obj: any): obj is IFormViewMediator {
  return obj.type ===  IViewType.Form;
}

/*export interface IFormView {
  type: IViewType.Form;
  init(): void;
  uiStructure: IUIFormRoot[];
  dataView: IDataViewMediator02;
  propReorder: IPropReorder;
  propCursor: IPropCursor;
  form: IForm;
  aSelNextProp: IASelNextProp;
  aSelPrevProp: IASelPrevProp;
  aActivateView: IAActivateView;
  aDeactivateView: IADeactivateView;
  aSelProp: IASelProp;
}*/

export interface IFormViewMachine {
  isActive: boolean;
  start(): void;
  stop(): void;
  send(event: any): void;
}