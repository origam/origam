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
import { IASelCell } from "../types/IASelCell";
import { IDataTable } from "../types/IDataTable";
import { IEditing } from "../types/IEditing";
import { IAFinishEditing } from "../types/IAFinishEditing";
import { IForm } from "../types/IForm";
import { action, computed } from "mobx";
import { Machine } from "xstate";
import { IDispatcher } from "../../utils/mediator";

import * as DataViewActions from "../DataViewActions";
import * as TableViewActions from "./TableViewActions";

import { IASelNextProp } from "../types/IASelNextProp";
import { IASelPrevProp } from "../types/IASelPrevProp";
import { IASelPrevRec } from "../types/IASelPrevRec";
import { IASelNextRec } from "../types/IASelNextRec";
import { IASelRec } from "../types/IASelRec";
import { IRecords } from "../types/IRecords";
import { ITableViewMachine } from "./types/ITableViewMachine";
import { START_DATA_VIEWS, STOP_DATA_VIEWS } from "../DataViewActions";
import { ISelection } from "../Selection";


export interface ITableViewMediator extends IDispatcher {
  type: IViewType.Table;
  isActive: boolean;
  propCursor: IPropCursor;
  propReorder: IPropReorder;
  properties: IProperties;
  records: IRecords;
  selection: ISelection;
  initPropIds: string[] | undefined;
  recCursor: IRecCursor;
  availViews: IAvailViews;
  dataTable: IDataTable;
  editing: IEditing;
  aFinishEditing: IAFinishEditing;
  form: IForm;
  dataView: IDataViewMediator02;

  aSelProp: IASelProp;
  aSelRec: IASelRec;
  aSelCell: IASelCell;
  aStartEditing: IAStartEditing;
  aActivateView: IAActivateView;
  aDeactivateView: IADeactivateView;
}

export class TableViewMediator implements ITableViewMediator, ITableView {
  type: IViewType.Table = IViewType.Table;

  constructor(
    public P: {
      initPropIds: string[] | undefined;
      parentMediator: IDataViewMediator02;
      machine: () => ITableViewMachine;
      propCursor: () => IPropCursor;
      propReorder: () => IPropReorder;
      selection: () => ISelection;
      aActivateView: () => IAActivateView;
      aDeactivateView: () => IADeactivateView;
      aSelProp: () => IASelProp;
      aSelRec: () => IASelRec;
      aSelCell: () => IASelCell;
      aSelNextProp: () => IASelNextProp;
      aSelPrevProp: () => IASelPrevProp;
      aSelNextRec: () => IASelNextRec;
      aSelPrevRec: () => IASelPrevRec;
    }
  ) {
    this.subscribeMediator();
  }

  subscribeMediator() {
    this.listen(event => {
      // TODO: Move this to a proper place? 

      /*if (isType(event, DataViewActions.selectCellByIdx)) {
        this.aSelCell.doByIdx(event.payload.rowIdx, event.payload.columnIdx);
      }*/ 
      
      /*else if (isType(event, TableViewActions.selectNextColumn)) {
        this.aSelNextProp.do();
        this.makeSelectedCellVisible();
      } else if (isType(event, TableViewActions.selectPrevColumn)) {
        this.aSelPrevProp.do();
        this.makeSelectedCellVisible();
      } /* else if (isType(event, DataViewActions.selectNextRow)) {
        console.log('@', event)
        this.aSelNextRec.do();
        this.makeSelectedCellVisible();
      } else if (isType(event, DataViewActions.selectPrevRow)) {
        this.aSelPrevRec.do();
        this.makeSelectedCellVisible();
    } */ /*if (isType(event, TableViewActions.makeCellVisibleById)) {
        // TODO: Move this to its own method.
        const columnIdx = this.propReorder.getIndexById(event.payload.columnId);
        const rowIdx = this.dataView.dataTable.getRecordIndexById(
          event.payload.rowId
        );
        if (columnIdx !== undefined && rowIdx !== undefined) {
          this.dispatch(
            TableViewActions.makeCellVisibleByIdx({ rowIdx, columnIdx })
          );
        }
      }*/
    });
  }

  @action.bound makeSelectedCellVisible() {
    const rowId = this.dataView.recCursor.selId;
    const columnId = this.propCursor.selId;
    if (rowId && columnId) {
      this.dispatch(TableViewActions.makeCellVisibleById({ rowId, columnId }));
    }
  }

  @computed get isActive(): boolean {
    return this.P.machine().isActive;
  }

  getParent(): IDispatcher {
    return this.P.parentMediator;
  }

  @action.bound dispatch(event: any) {
    if (event.NS === TableViewActions.NS) {
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

  downstreamDispatch(event: any): void {
    console.log("TableView received:", event);
    switch (event.type) {
      case START_DATA_VIEWS: {
        this.machine.start();
        break;
      }
      case STOP_DATA_VIEWS: {
        this.machine.stop();
        break;
      }
      case TableViewActions.SELECT_FIRST_CELL: {
        this.aSelCell.doSelFirst();
        break;
      }
    }
    for (let l of this.listeners.values()) {
      l(event);
    }
    this.machine.send(event);
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

  get records(): IRecords {
    return this.P.parentMediator.records;
  }

  get aSelRec(): IASelRec {
    return this.P.aSelRec();
  }

  get aStartEditing(): IAStartEditing {
    return this.P.parentMediator.aStartEditing;
  }

  get aSelCell(): IASelCell {
    return this.P.aSelCell();
  }

  get aSelNextProp() {
    return this.P.aSelNextProp();
  }

  get aSelPrevProp() {
    return this.P.aSelPrevProp();
  }

  get aSelNextRec() {
    return this.P.aSelNextRec();
  }

  get aSelPrevRec() {
    return this.P.aSelPrevRec();
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

  get machine(): ITableViewMachine {
    return this.P.machine();
  }

  get selection(): ISelection {
    return this.P.selection();
  }
}
