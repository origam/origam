import { action, reaction } from "mobx";
import { IApi } from "../Api/IApi";
import { IFormScreen } from "../Screens/FormScreen/types";
import { IDataSource } from "../Screens/types";
import { IDispatcher, stateVariableChanged } from "../utils/mediator";
import * as DataViewActions from "./DataViewActions";
import {
  ACTIVATE_INITIAL_VIEW_TYPES,
  START_DATA_VIEWS,
  STOP_DATA_VIEWS
} from "./DataViewActions";
import { IFormViewMediator } from "./FormView/FormViewMediator";
import { ITableViewMediator } from "./TableView/TableViewMediator";
import { IACancelEditing } from "./types/IACancelEditing";
import { IADeleteRow } from "./types/IADeleteRow";
import { IAFinishEditing } from "./types/IAFinishEditing";
import { IAInitForm } from "./types/IAInitForm";
import { IAReloadChildren } from "./types/IAReloadChildren";
import { IAStartEditing } from "./types/IAStartEditing";
import { IAStartView } from "./types/IAStartView";
import { IAStopView } from "./types/IAStopView";
import { IASubmitForm } from "./types/IASubmitForm";
import { IASwitchView } from "./types/IASwitchView";
import { IAvailViews } from "./types/IAvailViews";
import { IDataTable } from "./types/IDataTable";
import { IDataViewMachine } from "./types/IDataViewMachine";
import { IEditing } from "./types/IEditing";
import { IForm } from "./types/IForm";
import { IProperties } from "./types/IProperties";
import { IProperty } from "./types/IProperty";
import { IRecCursor } from "./types/IRecCursor";
import { IRecords } from "./types/IRecords";
import { IViewType } from "./types/IViewType";
import { start } from "xstate/lib/actions";
import { IAFocusEditor } from "./types/IAFocusEditor";

/* import { SELECT_FIRST_CELL } from "./FormView/FormViewActions";
import {
  selectFirstCell,
  SELECT_FIRST_CELL,
  ON_OUTSIDE_TABLE_CLICK,
  ON_NO_CELL_CLICK,
  ON_CELL_CLICK
} from "./TableView/TableViewActions"; */

export interface IParentMediator {
  api: IApi;
  menuItemId: string;
  getParent(): IDispatcher;
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
  isSessionedScreen: boolean;
  sessionId: string;
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
  aFocusEditor: IAFocusEditor;
}

export class DataViewMediator02 implements IDataViewMediator02 {
  constructor(
    public P: {
      parentMediator: IFormScreen;
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
      aFocusEditor: () => IAFocusEditor;
    }
  ) {}

  getParent(): IDispatcher {
    return this.P.parentMediator;
  }

  @action.bound dispatch(event: any) {
    if (event.NS === DataViewActions.NS) {
      this.downstreamDispatch(event);
      return;
    }
    switch (event.type) {
      default:
        this.getParent().dispatch(event);
    }
  }

  listeners = new Map<number, (event: any) => void>();
  idgen = 0;
  @action.bound
  listen(cb: (event: any) => void): () => void {
    const myId = this.idgen++;
    this.listeners.set(myId, cb);
    return () => this.listeners.delete(myId);
  }

  disposers: Array<() => void> = [];
  downstreamDispatch(event: any) {
    console.log("DataView received:", event);
    switch (event.type) {
      case START_DATA_VIEWS:
        this.start();
        break;
      case STOP_DATA_VIEWS:
        this.stop();
        break;
      case ACTIVATE_INITIAL_VIEW_TYPES:
        this.downstreamDispatch(
          DataViewActions.activateView({ viewType: this.initialActiveViewType })
        );
        break;
      case DataViewActions.START_EDITING:
        this.aStartEditing.do();
        break;
      case DataViewActions.FINISH_EDITING:
        this.aFinishEditing.do();
        break;
      case DataViewActions.CANCEL_EDITING:
        this.aCancelEditing.do();
        break;
      case DataViewActions.SWITCH_VIEW:
        this.aSwitchView.do(event.viewType);
        break;
      case DataViewActions.FOCUS_EDITOR:
        this.P.aFocusEditor().do();
        break;
    }
    this.machine.send(event);
    this.availViews.items.forEach(availView =>
      availView.downstreamDispatch(event)
    );
  }

  @action.bound start() {
    this.machine.start();
    this.disposers.push(
      reaction(
        () => [this.dataTable.hasContent],
        () => this.dispatch(stateVariableChanged())
      )
    );
  }

  @action.bound stop() {
    this.machine.stop();
    this.disposers.forEach(d => d());
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

  get isSessionedScreen(): boolean {
    return this.P.parentMediator.isSessioned;
  }

  get sessionId(): string {
    return this.P.parentMediator.sessionId;
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

  get aFocusEditor(): IAFocusEditor {
    return this.P.aFocusEditor();
  }
}
