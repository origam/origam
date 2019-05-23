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
import { Machine } from "xstate";
import { IDispatcher } from "../../utils/mediator";
import { action, computed } from "mobx";
import { START_DATA_VIEWS, STOP_DATA_VIEWS } from "../DataViewActions";
import { SELECT_FIRST_FIELD } from "./FormViewActions";

const activeTransitions = {
  ON_CREATE_ROW_CLICK: { actions: "onCreateRowClick" },
  ON_DELETE_ROW_CLICK: { actions: "onDeleteRowClick" },
  ON_SELECT_PREV_ROW_CLICK: { actions: "onSelectPrevRowClick" },
  ON_SELECT_NEXT_ROW_CLICK: { actions: "onSelectNextRowClick" },
  ON_FIELD_CLICK: { actions: "onFieldClick" },
  ON_NO_FIELD_CLICK: { actions: "onNoFieldClick" },
  ON_FORM_OUTSIDE_CLICK: { actions: "onFormOutsideClick" },
  ON_FIELD_KEY_DOWN: { actions: "onFieldKeyDown" },
  ON_FIELD_CHANGE: { actions: "onFieldChange" },

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

const formViewMachine = Machine(
  {
    initial: "INACTIVE",
    states: {
      INACTIVE: {
        on: {
          ACTIVATE: {
            target: "ACTIVE",
            cond: "isForMe"
          }
        }
      },
      ACTIVE: {
        initial: "WAIT_FOR_CONTENT",
        states: {
          WAIT_FOR_CONTENT: {
            on: {
              "": { cond: "dataTableHasContent", target: "RUNNING" },
              DATA_TABLE_LOADED: {
                cond: "dataTableHasContent",
                target: "RUNNING"
              }
            }
          },
          RUNNING: {
            on: {
              ...activeTransitions
            }
          }
        },
        on: {
          ACTIVATE: {
            target: "INACTIVE",
            cond: "isNotForMe"
          }
        }
      }
    }
  },
  {
    guards: {
      dataTableHasContent: (ctx: any, event: any) => {
        return false;
      },
      isForMe: (ctx: any, event: any) => {
        return false;
      },
      isNotForMe: (ctx: any, event: any) => {
        return false;
      }
    },
    actions: {
      onCreateRowClick: (ctx: any, event: any) => {},
      onDeleteRowClick: (ctx: any, event: any) => {},
      onSelectPrevRowClick: (ctx: any, event: any) => {},
      onSelectNextRowClick: (ctx: any, event: any) => {},
      onFieldClick: (ctx: any, event: any) => {},
      onNoFieldClick: (ctx: any, event: any) => {},
      onFormOutsideClick: (ctx: any, event: any) => {},
      onFieldKeyDown: (ctx: any, event: any) => {},
      onFieldChange: (ctx: any, event: any) => {}
    }
  }
);

export interface IParentMediator {
  properties: IProperties;
  records: IRecords;
  recCursor: IRecCursor;
  editing: IEditing;
  availViews: IAvailViews;
  form: IForm;
  dataTable: IDataTable;
}

export interface IFormViewMediator extends IDispatcher {
  type: IViewType.Form;

  isActive: boolean;
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
  ) {
    this.subscribeMediator();
  }

  subscribeMediator() {}

  getParent(): IDispatcher {
    return this.P.parentMediator;
  }

  @action.bound dispatch(event: any) {
    switch (event.type) {
      case SELECT_FIRST_FIELD:
        this.downstreamDispatch(event);
        break;
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
    console.log("FormView received:", event);
    switch (event.type) {
      case START_DATA_VIEWS: {
        this.machine.start();
        break;
      }
      case STOP_DATA_VIEWS: {
        this.machine.stop();
        break;
      }
      case SELECT_FIRST_FIELD: {
        this.aSelCell.doSelFirst();
        break;
      }
    }
    for (let l of this.listeners.values()) {
      l(event);
    }
    this.machine.send(event);
  }

  @computed
  get isActive(): boolean {
    return this.P.machine().isActive;
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

  get aSelProp(): IASelProp {
    return this.P.aSelProp();
  }

  get aSelCell(): IASelCell {
    return this.P.aSelCell();
  }

  get aActivateView(): IAActivateView {
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
