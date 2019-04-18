import { IFormViewFactory, IFormView, IUIFormRoot } from "./types";
import { FormViewPresenter } from "./FormViewPresenter";
import { FormViewToolbar } from "./FormViewToolbar";

export class FormViewPresenterFactory implements IFormViewFactory {
  create(uiStructure: IUIFormRoot[]): IFormView {
    throw new Error("Not implemented.")
  }
}
