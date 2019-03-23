import { IViewType } from "../IScreenPresenter";
import { IUIFormRoot } from "../IUIScreenBlueprints";
import { IToolbar } from "../ITableViewPresenter/IToolbar";
import { IFormField } from "./IFormField";


export interface IFormView {
  type: IViewType.FormView;
  uiStructure: IUIFormRoot[];
  toolbar: IToolbar | undefined;
  fields: Map<string, IFormField>;
  onFieldClick?(event: any, id: string): void;
  onFieldFocus?(event: any, id: string): void;
  onFieldBlur?(event: any, id: string): void;
  onFieldChange?(event: any, id: string, value: any): void;
  onFieldKeyDown?(event: any, id: string): void;
  onFieldOutsideClick?(event: any, id: string): void;
}
