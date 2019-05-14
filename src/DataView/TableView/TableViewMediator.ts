import { IPropCursor } from "../types/IPropCursor";
import { IPropReorder } from "../types/IPropReorder";
import { IProperties } from "../types/IProperties";
import { ITableView } from "./ITableView";
import { IViewType } from "../types/IViewType";
import { IRecCursor } from "../types/IRecCursor";
import { IDataViewMediator02 } from "../DataViewMediator02";

import { IADeactivateView } from "../types/IADeactivateView";
import { IAActivateView } from "../types/IAActivateView";
import { IASelProp } from "../types/IASelProp";
import { IAStartEditing } from "../types/IAStartEditing";
import { IAvailViews } from "../types/IAvailViews";
import { IFormViewMachine } from "../FormView/types";
import { IASelCell } from "../types/IASelCell";
import { IDataTable } from "../types/IDataTable";
import { IEditing } from "../types/IEditing";
import { IAFinishEditing } from "../types/IAFinishEditing";
import { IForm } from "../types/IForm";
import { action } from "mobx";

export interface ITableViewMediator {
  type: IViewType.Table;
  propCursor: IPropCursor;
  propReorder: IPropReorder;
  properties: IProperties;
  initPropIds: string[] | undefined;
  recCursor: IRecCursor;
  aSelProp: IASelProp;
  aSelCell: IASelCell;
  aStartEditing: IAStartEditing;
  aActivateView: IAActivateView;
  aDeactivateView: IADeactivateView;
  availViews: IAvailViews;
  dataTable: IDataTable;
  editing: IEditing;
  aFinishEditing: IAFinishEditing;
  form: IForm;
  dispatch(action: any): void;
  listen(cb: (action: any) => void): void;
}

export class TableViewMediator implements ITableViewMediator, ITableView {

  type: IViewType.Table = IViewType.Table;


  constructor(
    public P: {
      initPropIds: string[] | undefined;
      parentMediator: IDataViewMediator02;
      propCursor: () => IPropCursor;
      propReorder: () => IPropReorder;
      aActivateView: () => IAActivateView;
      aDeactivateView: () => IADeactivateView;
      aSelProp: () => IASelProp;
      aSelCell: () => IASelCell;
    }
  ) {}

  @action.bound
  dispatch(action: any): void {
    throw new Error("Method not implemented.");
  }

  @action.bound
  listen(cb: (action: any) => void): void {
    throw new Error("Method not implemented.");
  }

  get dataView(): IDataViewMediator02 {
    return this.P.parentMediator;
  }

  get recCursor(): IRecCursor {
    return this.P.parentMediator.recCursor;
  }

  get propCursor(): IPropCursor {
    return this.P.propCursor();
  }

  get propReorder(): IPropReorder {
    return this.P.propReorder();
  }

  get initPropIds(): string[] | undefined {
    return this.P.initPropIds;
  }

  get properties() {
    return this.P.parentMediator.properties;
  }

  get aActivateView(): IAActivateView {
    return this.P.aActivateView();
  }

  get aDeactivateView(): IADeactivateView {
    return this.P.aDeactivateView();
  }

  get aSelProp(): IASelProp {
    return this.P.aSelProp();
  }

  get aStartEditing(): IAStartEditing {
    return this.P.parentMediator.aStartEditing;
  }

  get aSelCell(): IASelCell {
    return this.P.aSelCell();
  }

  get availViews(): IAvailViews {
    return this.P.parentMediator.availViews;
  }

  get dataTable(): IDataTable {
    return this.P.parentMediator.dataTable;
  }

  get editing(): IEditing {
    return this.P.parentMediator.editing;
  }

  get aFinishEditing(): IAFinishEditing {
    return this.P.parentMediator.aFinishEditing;
  }

  get form(): IForm {
    return this.P.parentMediator.form;
  }
}
