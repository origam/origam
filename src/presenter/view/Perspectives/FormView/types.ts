import { IViewType } from "../../../../DataView/types/IViewType";
import { IToolbar, ICellTypeDU } from "../types";

export interface IFormView {
  type: IViewType.Form;
  uiStructure: Array<IUIFormRoot>;
  toolbar: IToolbar | undefined;
  fields: Map<string, IFormField>;
  isLoading: boolean;
  onNoFieldClick?(event: any): void;
  onOutsideFormClick?(event: any): void;
  /*onFieldClick?(event: any, id: string): void;
  onFieldFocus?(event: any, id: string): void;
  onFieldBlur?(event: any, id: string): void;
  onFieldChange?(event: any, id: string, value: any): void;
  onFieldKeyDown?(event: any, id: string): void;
  onFieldOutsideClick?(event: any, id: string): void;*/
}

interface IField {
  isFocused: boolean;
  isLoading: boolean;
  isInvalid: boolean;
  isReadOnly: boolean;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  refocuser?: (cb: () => void) => () => void;
}

export type IFormField = ICellTypeDU & IField;

export type IUIFormViewTreeNode = IUIFormRoot | IUIFormSection | IUIFormField;

export interface IUIFormRoot {
  type: "FormRoot";
  props: { Type: string };
  children: Array<IUIFormField | IUIFormSection>;
}

export interface IUIFormSection {
  type: "FormSection";
  props: {
    Width: string;
    Height: string;
    Y: string;
    X: string;
    Title: string;
  };
  children: IUIFormField[];
}

export interface IUIFormField {
  type: "Property";
  props: {
    Id: string;
    Name: string;
    CaptionLength: string;
    CaptionPosition: ICaptionPosition;
    Column: string;
    Entity: string;
    Height: string;
    Width: string;
    Y: string;
    X: string;
  };
  children: [];
}

export enum ICaptionPosition {
  Left = "Left",
  Right = "Right",
  Top = "Top",
  None = "None"
}

export interface IFormViewFactory {
  create(uiStructure: IUIFormRoot[]): IFormView;
}
