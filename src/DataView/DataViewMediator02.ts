import { IDataTable } from "./types/IDataTable";
import { IEditing } from "./types/IEditing";
import { IAvailViews } from "./types/IAvailViews";
import { IRecCursor } from "./types/IRecCursor";
import { IForm } from "./types/IForm";
import { IRecords } from "./types/IRecords";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { IProperties } from "./types/IProperties";
import { IApi } from "../Api/IApi";
import { IDataSource } from "../Screens/types";
import { IViewType } from "./types/IViewType";
import { IProperty } from "./types/IProperty";
import { IAStartView } from "./types/IAStartView";
import { IAStopView } from "./types/IAStopView";
import { IAStartEditing } from "./types/IAStartEditing";
import { IAFinishEditing } from "./types/IAFinishEditing";
import { IACancelEditing } from "./types/IACancelEditing";
import { IASwitchView } from "./types/IASwitchView";
import { IAInitForm } from "./types/IAInitForm";
import { IASubmitForm } from "./types/IASubmitForm";
import { IAReloadChildren } from "./types/IAReloadChildren";
import { IADeleteRow } from "./types/IADeleteRow";
import { action } from "mobx";
import { ITableViewMediator } from "./TableView/TableViewMediator";
import { IFormViewMediator } from "./FormView/FormViewMediator";
import { Machine } from "xstate";
import { IDispatcher } from "../utils/mediator";

export const dataViewMachine02 = Machine({
  states: {}
});

export interface IParentMediator {
  api: IApi;
  menuItemId: string;
}

export interface ISelection {
  selRowId: string | undefined;
  selColId: string | undefined;
  selRowIdx: number | undefined;
  selColIdx: number | undefined;
}

export interface IDataViewMediator02 extends IDispatcher {
  editing: IEditing;
  dataTable: IDataTable;
  availViews: IAvailViews;
  recCursor: IRecCursor;
  form: IForm;
  records: IRecords;
  properties: IProperties;
  machine: IDataViewMachine;

  availViewItems: Array<IFormViewMediator | ITableViewMediator>;
  specificDataViews: Array<IFormViewMediator | ITableViewMediator>;
  initialDataView: IViewType;
  initialActiveViewType: IViewType;
  id: string;
  api: IApi;
  menuItemId: string;
  dataStructureEntityId: string;
  isHeadless: boolean;
  label: string;
  propertyIdsToLoad: string[];
  propertyItems: IProperty[];
  dataSource: IDataSource;
  selectedIdGetter: () => string | undefined;

  aStartView: IAStartView;
  aStopView: IAStopView;
  aStartEditing: IAStartEditing;
  aFinishEditing: IAFinishEditing;
  aCancelEditing: IACancelEditing;
  aSwitchView: IASwitchView;
  aInitForm: IAInitForm;
  aSubmitForm: IASubmitForm;
  aReloadChildren: IAReloadChildren;
  aDeleteRow: IADeleteRow;
}

export class DataViewMediator02 implements IDataViewMediator02 {
  constructor(
    public P: {
      parentMediator: IParentMediator;
      id: string;
      label: string;
      isHeadless: boolean;
      availViewItems: () => Array<IFormViewMediator | ITableViewMediator>;
      propertyItems: () => IProperty[];
      initialActiveViewType: IViewType;
      dataStructureEntityId: string;
      dataSource: IDataSource;
      editing: () => IEditing;
      dataTable: () => IDataTable;
      availViews: () => IAvailViews;
      recCursor: () => IRecCursor;
      form: () => IForm;
      records: () => IRecords;
      properties: () => IProperties;
      machine: () => IDataViewMachine;

      aStartView: () => IAStartView;
      aStopView: () => IAStopView;
      aStartEditing: () => IAStartEditing;
      aFinishEditing: () => IAFinishEditing;
      aCancelEditing: () => IACancelEditing;
      aSwitchView: () => IASwitchView;
      aInitForm: () => IAInitForm;
      aSubmitForm: () => IASubmitForm;
      aReloadChildren: () => IAReloadChildren;
      aDeleteRow: () => IADeleteRow;
    }
  ) {}

  getRoot() {
    return this;
  }

  @action.bound
  dispatch(event: any) {
    this.getRoot().downstreamDispatch(event);
  }

  listeners = new Map<number, (event: any) => void>();
  idgen = 0;
  @action.bound
  listen(cb: (event: any) => void): () => void {
    const myId = this.idgen++;
    this.listeners.set(myId, cb);
    return () => this.listeners.delete(myId);
  }

  downstreamDispatch(event: any) {
    console.log("FormView received:", event);
    for (let l of this.listeners.values()) {
      l(event);
    }
    this.availViews.items.forEach(availView =>
      availView.downstreamDispatch(event)
    );
  }

  get editing(): IEditing {
    return this.P.editing();
  }

  get dataTable(): IDataTable {
    return this.P.dataTable();
  }

  get availViews(): IAvailViews {
    return this.P.availViews();
  }

  get recCursor(): IRecCursor {
    return this.P.recCursor();
  }

  get form(): IForm {
    return this.P.form();
  }

  get records(): IRecords {
    return this.P.records();
  }

  get properties(): IProperties {
    return this.P.properties();
  }

  get machine(): IDataViewMachine {
    return this.P.machine();
  }

  get api(): IApi {
    return this.P.parentMediator.api;
  }

  get menuItemId(): string {
    return this.P.parentMediator.menuItemId;
  }

  get dataStructureEntityId(): string {
    return this.P.dataStructureEntityId;
  }

  get propertyIdsToLoad(): string[] {
    return this.properties.ids;
  }

  get dataSource(): IDataSource {
    return this.P.dataSource;
  }

  selectedIdGetter(): string | undefined {
    return this.recCursor.selId;
  }

  get availViewItems(): (IFormViewMediator | ITableViewMediator)[] {
    return this.P.availViewItems();
  }

  get initialActiveViewType(): IViewType {
    return this.P.initialActiveViewType;
  }

  get initialDataView(): IViewType {
    return this.P.initialActiveViewType;
  }

  get propertyItems(): IProperty[] {
    return this.P.propertyItems();
  }

  get id(): string {
    return this.P.id;
  }

  get label(): string {
    return this.P.label;
  }

  get isHeadless(): boolean {
    return this.P.isHeadless;
  }

  get specificDataViews(): (IFormViewMediator | ITableViewMediator)[] {
    return this.P.availViewItems();
  }

  get aStartView(): IAStartView {
    return this.P.aStartView();
  }

  get aStopView(): IAStopView {
    return this.P.aStopView();
  }

  get aStartEditing(): IAStartEditing {
    return this.P.aStartEditing();
  }

  get aFinishEditing(): IAFinishEditing {
    return this.P.aFinishEditing();
  }

  get aCancelEditing(): IACancelEditing {
    return this.P.aCancelEditing();
  }

  get aSwitchView(): IASwitchView {
    return this.P.aSwitchView();
  }

  get aInitForm(): IAInitForm {
    return this.P.aInitForm();
  }

  get aSubmitForm(): IASubmitForm {
    return this.P.aSubmitForm();
  }

  get aReloadChildren(): IAReloadChildren {
    return this.P.aReloadChildren();
  }

  get aDeleteRow(): IADeleteRow {
    return this.P.aDeleteRow();
  }
}
