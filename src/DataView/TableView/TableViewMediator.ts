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
import { action } from "mobx";
import { Machine } from "xstate";
import { IDispatcher } from "../../utils/mediator";
import { isType } from "ts-action";

import * as DataViewActions from "../DataViewActions";
import * as TableViewActions from "./TableViewActions";

import { IASelNextProp } from "../types/IASelNextProp";
import { IASelPrevProp } from "../types/IASelPrevProp";
import { IASelPrevRec } from "../types/IASelPrevRec";
import { IASelNextRec } from "../types/IASelNextRec";
import { IASelRec } from "../types/IASelRec";
import { IRecords } from "../types/IRecords";

const activeTransitions = {
  ON_CREATE_ROW_CLICK: {},
  ON_DELETE_ROW_CLICK: {},
  ON_SELECT_PREV_ROW_CLICK: {},
  ON_SELECT_NEXT_ROW_CLICK: {},
  ON_CELL_CLICK: {},
  ON_NO_CELL_CLICK: {},
  ON_TABLE_OUTSIDE_CLICK: {},
  ON_CELL_KEY_DOWN: {},
  ON_CELL_CHANGE: {},

  START_EDITING: {}, // ?
  CANCEL_EDITING: {}, // ?
  FINISH_EDITING: {}, // ?
  SELECT_PREV_ROW: {}, // ?
  SELECT_NEXT_ROW: {}, // ?
  SELECT_PREV_CELL: {}, // ?
  SELECT_NEXT_CELL: {}, // ?
  SELECT_CELL: {}, // ?
  MAKE_CELL_VISIBLE: {} // ?
};

const tableViewMachine = Machine({
  initial: "INACTIVE",
  states: {
    ACTIVE: {
      on: {
        ACTIVATE: {
          target: "INACTIVE",
          cond: "isNotForMe"
        },
        ...activeTransitions
      }
    },
    INACTIVE: {
      on: {
        ACTIVATE: {
          target: "ACTIVE",
          cond: "isForMe"
        }
      }
    }
  }
});

export interface ITableViewMediator extends IDispatcher {
  type: IViewType.Table;
  propCursor: IPropCursor;
  propReorder: IPropReorder;
  properties: IProperties;
  records: IRecords;
  initPropIds: string[] | undefined;
  recCursor: IRecCursor;
  availViews: IAvailViews;
  dataTable: IDataTable;
  editing: IEditing;
  aFinishEditing: IAFinishEditing;
  form: IForm;

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
      propCursor: () => IPropCursor;
      propReorder: () => IPropReorder;
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
      if (isType(event, DataViewActions.selectFirstCell)) {
        this.aSelCell.doSelFirst();
      } else if (isType(event, DataViewActions.selectCellByIdx)) {
        this.aSelCell.doByIdx(event.payload.rowIdx, event.payload.columnIdx);
      } else if (isType(event, DataViewActions.selectNextColumn)) {
        this.aSelNextProp.do();
        this.makeSelectedCellVisible();
      } else if (isType(event, DataViewActions.selectPrevColumn)) {
        this.aSelPrevProp.do();
        this.makeSelectedCellVisible();
      } else if (isType(event, DataViewActions.selectNextRow)) {
        this.aSelNextRec.do();
        this.makeSelectedCellVisible();
      } else if (isType(event, DataViewActions.selectPrevRow)) {
        this.aSelPrevRec.do();
        this.makeSelectedCellVisible();
      } else if (isType(event, TableViewActions.makeCellVisibleById)) {
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
      }
    });
  }

  @action.bound makeSelectedCellVisible() {
    const rowId = this.dataView.recCursor.selId;
    const columnId = this.propCursor.selId;
    if (rowId && columnId) {
      this.dispatch(TableViewActions.makeCellVisibleById({ rowId, columnId }));
    }
  }

  dispatch(event: any): void {
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

  getRoot(): IDispatcher {
    return this.P.parentMediator.getRoot();
  }

  downstreamDispatch(event: any): void {
    console.log("TableView received:", event);
    for (let l of this.listeners.values()) {
      l(event);
    }
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
}
