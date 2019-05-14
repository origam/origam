import { IPropReorder } from "../types/IPropReorder";
import { IPropCursor } from "../types/IPropCursor";
import { IFormViewMachine } from "./types";
import { IProperties } from "../types/IProperties";
import { IRecords } from "../types/IRecords";
import { IRecCursor } from "../types/IRecCursor";
import { IEditing } from "../types/IEditing";
import { IAvailViews } from "../types/IAvailViews";
import { IForm } from "../types/IForm";
import { IDataTable } from "../types/IDataTable";
import { IDataViewMediator02 } from "../DataViewMediator02";
import { IASelPrevProp } from "../types/IASelPrevProp";
import { IASelNextProp } from "../types/IASelNextProp";
import { IASelProp } from "../types/IASelProp";
import { IAActivateView } from "../types/IAActivateView";
import { IADeactivateView } from "../types/IADeactivateView";
import { IViewType } from "../types/IViewType";
import { IAStartEditing } from "../types/IAStartEditing";
import { IAFinishEditing } from "../types/IAFinishEditing";
import { IASelCell } from "../types/IASelCell";

export interface IParentMediator {
  properties: IProperties;
  records: IRecords;
  recCursor: IRecCursor;
  editing: IEditing;
  availViews: IAvailViews;
  form: IForm;
  dataTable: IDataTable;
}

export interface IFormViewMediator {
  type: IViewType.Form;

  initPropIds: string[] | undefined;
  propReorder: IPropReorder;
  propCursor: IPropCursor;
  machine: IFormViewMachine;

  properties: IProperties;
  records: IRecords;
  recCursor: IRecCursor;
  editing: IEditing;
  availViews: IAvailViews;
  form: IForm;
  dataTable: IDataTable;
  dataView: IDataViewMediator02;
  uiStructure: any[];
  aSelPrevProp: IASelPrevProp;
  aSelNextProp: IASelNextProp;
  aSelProp: IASelProp;
  aSelCell: IASelCell;
  aActivateView: IAActivateView;
  aDeactivateView: IADeactivateView;
  aStartEditing: IAStartEditing;
  aFinishEditing: IAFinishEditing;

  dispatch(action: any): void;
  listen(cb: (action: any) => void): void;
}

export class FormViewMediator implements IFormViewMediator {
  

  type: IViewType.Form = IViewType.Form;

  constructor(
    public P: {
      initPropIds: string[] | undefined;
      uiStructure: any[];
      parentMediator: IDataViewMediator02;
      propReorder: () => IPropReorder;
      propCursor: () => IPropCursor;
      machine: () => IFormViewMachine;
      aSelNextProp: () => IASelNextProp;
      aSelPrevProp: () => IASelPrevProp;
      aSelProp: () => IASelProp;
      aSelCell: () => IASelCell;
      aActivateView: () => IAActivateView;
      aDeactivateView: () => IADeactivateView;
    }
  ) {}

  dispatch(action: any): void {
    throw new Error("Method not implemented.");
  }

  listen(cb: (action: any) => void): void {
    throw new Error("Method not implemented.");
  }

  get propReorder(): IPropReorder {
    return this.P.propReorder();
  }

  get propCursor(): IPropCursor {
    return this.P.propCursor();
  }

  get machine(): IFormViewMachine {
    return this.P.machine();
  }

  get properties(): IProperties {
    return this.P.parentMediator.properties;
  }

  get records(): IRecords {
    return this.P.parentMediator.records;
  }

  get recCursor(): IRecCursor {
    return this.P.parentMediator.recCursor;
  }

  get editing(): IEditing {
    return this.P.parentMediator.editing;
  }

  get availViews(): IAvailViews {
    return this.P.parentMediator.availViews;
  }

  get form(): IForm {
    return this.P.parentMediator.form;
  }

  get dataTable(): IDataTable {
    return this.P.parentMediator.dataTable;
  }

  get initPropIds(): string[] | undefined {
    return this.P.initPropIds;
  }

  get dataView(): IDataViewMediator02 {
    return this.P.parentMediator;
  }

  get uiStructure(): any[] {
    return this.P.uiStructure;
  }

  get aSelPrevProp(): IASelPrevProp {
    return this.P.aSelPrevProp();
  }

  get aSelNextProp(): IASelNextProp {
    return this.P.aSelNextProp();
  }

  get aSelProp() : IASelProp {
    return this.P.aSelProp();
  }

  get aSelCell(): IASelCell {
    return this.P.aSelCell();
  }

  get aActivateView(): IAActivateView{
    return this.P.aActivateView();
  }

  get aDeactivateView(): IADeactivateView {
    return this.P.aDeactivateView();
  }

  get aStartEditing(): IAStartEditing {
    return this.P.parentMediator.aStartEditing;
  }

  get aFinishEditing(): IAFinishEditing {
    return this.P.parentMediator.aFinishEditing;
  }
}
